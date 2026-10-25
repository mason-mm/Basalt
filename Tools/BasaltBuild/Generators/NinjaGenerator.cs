using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BasaltBuild.Util;

/// <summary>
/// Generates the required files to build using Ninja.
/// </summary>
namespace BasaltBuild.Generators;

/// <summary>
/// Generates Ninja files for building the project.
/// </summary>
public class NinjaGenerator : BuildGenerator
{
    /// <inheritdoc />
    public override void ExecuteBuild(string rootDir, BuildConfiguration? config = null)
    {
        // Get the config.
        BuildConfiguration activeConfig = config ?? BuildConfiguration.Debug;
        Console.WriteLine($"\n[BasaltBuild] Triggering Ninja ({activeConfig})...");

        string ninjaFileName = GetNinjaFileName(activeConfig);
        string projectFilesDir = Path.Combine(rootDir, "Intermediate", "ProjectFiles");
        string ninjaPath = Path.Combine(projectFilesDir, ninjaFileName);

        if (!File.Exists(ninjaPath))
        {
            Console.WriteLine($"[BasaltBuild] Warning: '{ninjaFileName}' not found in Intermediate/ProjectFiles/. Please run configure first.");
            return;
        }

        // Setup the process.
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "ninja",
            Arguments = $"-f Intermediate/ProjectFiles/{ninjaFileName} -v",
            WorkingDirectory = rootDir,
            UseShellExecute = false
        };

        // Start the process.
        var process = Process.Start(psi);
        process?.WaitForExit();
    }

    /// <inheritdoc />
    public override void Configure(string rootDir, List<(string ModulePath, ModuleData Data)> modules, TargetPlatform target)
    {
        Console.WriteLine("[BasaltBuild] Configuring Ninja build manifests for all configurations...");

        // Filter modules based on target platform compatibility
        var activeModules = modules.Where(m =>
            m.Data.Platforms == null ||
            m.Data.Platforms.Count == 0 ||
            m.Data.Platforms.Contains(target.ToString(), StringComparer.OrdinalIgnoreCase)
        ).ToList();

        // Loops through all configs in the enum and generates the Ninja file for it.
        foreach (BuildConfiguration config in Enum.GetValues<BuildConfiguration>())
        {
            GenerateManifestForConfig(rootDir, activeModules, target, config);
        }
    }

    /// <summary>
    /// Generates a configuration specific build manifest file.
    /// </summary>
    /// <param name="rootDir">The directory to start the build form (CWD).</param>
    /// <param name="modules">The list of active modules to build.</param>
    /// <param name="target">The target platform to build for.</param>
    /// <param name="config">The build config to build for like debug or release.</param>
    private static void GenerateManifestForConfig(string rootDir, List<(string ModulePath, ModuleData Data)> modules, TargetPlatform target, BuildConfiguration config)
    {
        string ninjaFileName = GetNinjaFileName(config);
        string projectFilesDir = Path.Combine(rootDir, "Intermediate", "ProjectFiles");
        Directory.CreateDirectory(projectFilesDir);
        string ninjaPath = Path.Combine(projectFilesDir, ninjaFileName);

        StringBuilder sb = new();

        sb.AppendLine($"# ======================================================================");
        sb.AppendLine($"# Auto-generated Ninja Build Manifest for [{config}] by BasaltBuild");
        sb.AppendLine($"# ======================================================================");
        sb.AppendLine("builddir = Intermediate/ProjectFiles"); // Keeps .ninja_log out of root
        sb.AppendLine();

        // Map enum configurations to MSVC optimization and debug symbol flags.
        string optimizationFlags = config switch
        {
            BuildConfiguration.Debug => "/Od /Zi /RTC1",
            BuildConfiguration.Release => "/O2 /Zi /DNDEBUG",
            BuildConfiguration.Development => "/O2 /Zi",
            BuildConfiguration.Shipping => "/O2 /DNDEBUG",
            _ => "/Od"
        };

        // Add compiler and linker rules.
        sb.AppendLine("cxx = cl");
        sb.AppendLine("archiver = lib");
        sb.AppendLine("linker = link");
        sb.AppendLine();

        sb.AppendLine("rule cxx_compile");
        sb.AppendLine($"  command = $cxx /nologo $langflag /FS {optimizationFlags} $cppstd /Fd$pdbpath /showIncludes $defines $includes /c $in /Fo$out");
        sb.AppendLine("  description = Compiling $out");
        sb.AppendLine();

        sb.AppendLine("rule archive_lib");
        sb.AppendLine("  command = $archiver /nologo /out:$out $in");
        sb.AppendLine("  description = Archiving static library $out");
        sb.AppendLine();

        sb.AppendLine("rule link_dll");
        sb.AppendLine("  command = $linker /nologo /DLL /out:$out $in $libs /IMPLIB:$implib");
        sb.AppendLine("  description = Linking dynamic library $out");
        sb.AppendLine();

        sb.AppendLine("rule link_exe");
        sb.AppendLine("  command = $linker /nologo /out:$out $in $libs");
        sb.AppendLine("  description = Linking executable $out");
        sb.AppendLine();

        string binDir = Path.Combine("Binaries", target.ToString(), config.ToString());

        foreach (var (moduleDir, module) in modules)
        {
            sb.AppendLine($"# --- Module: {module.Name} [{config}] ---");

            // Collect both .cpp and .c source files
            var sourceFiles = GetFiles(moduleDir, module.SourceDirectories ?? [], "*.cpp")
                .Concat(GetFiles(moduleDir, module.SourceDirectories ?? [], "*.c"))
                .ToList();

            string moduleObjDir = Path.Combine(rootDir, "Intermediate", "Build", target.ToString(), config.ToString(), module.Name);
            Directory.CreateDirectory(moduleObjDir);
            string relModuleObjDir = Path.GetRelativePath(rootDir, moduleObjDir);

            // Collect module's own include directories
            var includeDirs = new List<string>();
            foreach (var d in module.IncludeDirectories ?? [])
            {
                includeDirs.Add(Path.Combine(moduleDir, d));
            }

            // Filter dependencies to only include active modules
            var validDeps = (module.Dependencies ?? []).Where(dep => modules.Any(m => m.Data.Name == dep)).ToList();

            // Collect include directories from dependent modules
            foreach (var depName in validDeps)
            {
                var depModule = modules.FirstOrDefault(m => m.Data.Name == depName);
                if (depModule.Data != null)
                {
                    foreach (var depInc in depModule.Data.IncludeDirectories ?? [])
                    {
                        includeDirs.Add(Path.Combine(depModule.ModulePath, depInc));
                    }
                }
            }

            string includes = string.Join(" ", includeDirs.Select(d => $"/I\"{d}\""));
            string defines = string.Join(" ", (module.Defines ?? []).Select(def => $"/D{def}"));
            string cppStd = $"/std:c++{module.CppVersion}";

            var objFiles = new List<string>();

            foreach (var sourceFile in sourceFiles)
            {
                string relSource = Path.GetRelativePath(rootDir, sourceFile);
                string objName = Path.GetFileNameWithoutExtension(sourceFile) + ".obj";
                string objPath = Path.Combine(moduleObjDir, objName);
                string relObj = Path.GetRelativePath(rootDir, objPath);
                objFiles.Add(relObj);

                // Use /TC for C source files and /TP for C++ source files
                string langflag = sourceFile.EndsWith(".c", StringComparison.OrdinalIgnoreCase) ? "/TC" : "/TP";

                sb.AppendLine($"build {relObj}: cxx_compile {relSource}");
                sb.AppendLine($"  includes = {includes}");
                sb.AppendLine($"  defines = {defines}");
                sb.AppendLine($"  cppstd = {cppStd}");
                sb.AppendLine($"  langflag = {langflag}");
                sb.AppendLine($"  pdbpath = {relModuleObjDir}\\\\"); // Directs vc140.pdb into intermediate build dir
            }

            // Determine output binary path matching MSBuild OutDir
            string outputBinPath = module.Type switch
            {
                "DynamicLibrary" => Path.Combine(binDir, $"{module.Name}.dll"),
                "Executable" => Path.Combine(binDir, $"{module.Name}.exe"),
                _ => Path.Combine(binDir, $"{module.Name}.lib")
            };

            string objList = string.Join(" ", objFiles);

            // Collect dependency .lib paths as explicit inputs to ensure correct build ordering
            string depLibs = string.Join(" ", validDeps.Select(dep => Path.Combine(binDir, $"{dep}.lib")));

            if (module.Type == "DynamicLibrary")
            {
                string implibPath = Path.Combine(binDir, $"{module.Name}.lib");
                sb.AppendLine($"build {outputBinPath} | {implibPath}: link_dll {objList} {depLibs}");
                sb.AppendLine($"  libs = {depLibs}");
                sb.AppendLine($"  implib = {implibPath}");
            }
            else if (module.Type == "Executable")
            {
                sb.AppendLine($"build {outputBinPath}: link_exe {objList} {depLibs}");
                sb.AppendLine($"  libs = {depLibs}");
            }
            else
            {
                sb.AppendLine($"build {outputBinPath}: archive_lib {objList}");
            }

            sb.AppendLine();
        }

        // Clean up.
        File.WriteAllText(ninjaPath, sb.ToString());
        Console.WriteLine($" -> Generated: Intermediate/ProjectFiles/{ninjaFileName}");
    }

    /// <summary>
    /// Returns the string name for a Ninja Config File.
    /// </summary>
    /// <param name="config">The build config to build for like debug or release.</param>
    /// <returns>The file name.</returns>
    private static string GetNinjaFileName(BuildConfiguration config)
    {
        return $"build_{config}.ninja";
    }
}
