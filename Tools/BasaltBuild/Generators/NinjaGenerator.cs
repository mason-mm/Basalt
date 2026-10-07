using System.Diagnostics;
using System.Text;
using BasaltBuild.Core;

namespace BasaltBuild.Generators;

public static class NinjaGenerator
{
    public static void GenerateNinjaFile(string rootDir, List<(string ModulePath, ModuleData Data)> modules, TargetPlatform platform, BuildConfiguration config)
    {
        string ninjaPath = Path.Combine(rootDir, "build.ninja");
        StringBuilder sb = new StringBuilder();
        string compiler = platform == TargetPlatform.Mac ? "clang++" : "g++";
        string optFlag = (config == BuildConfiguration.Debug) ? "-g -O0" : "-O3 -DNDEBUG";

        sb.AppendLine("# Basalt Native Cross-Platform Ninja Build Manifest");
        sb.AppendLine($"cxx = {compiler}");
        sb.AppendLine($"flags = -std=c++20 -Wall {optFlag}");
        sb.AppendLine();
        sb.AppendLine("rule cxx");
        sb.AppendLine("  command = $cxx $flags $defines $includes -c $in -o $out");
        sb.AppendLine("  description = CXX $out");
        sb.AppendLine();
        sb.AppendLine("rule link_so");
        sb.AppendLine("  command = $cxx -shared $in -o $out");
        sb.AppendLine("  description = LINK_SHARED $out");
        sb.AppendLine();
        sb.AppendLine("rule link_exe");
        sb.AppendLine("  command = $cxx $in -o $out");
        sb.AppendLine("  description = LINK_EXE $out");
        sb.AppendLine();

        List<string> allTargets = new();

        foreach (var (moduleDir, data) in modules)
        {
            var cppFiles = GetSourceFiles(moduleDir, data.SourceDirectories, "*.cpp");
            var objectFiles = new List<string>();

            string incFlags = string.Join(" ", data.IncludeDirectories.Select(i => $"-I{NormalizePath(Path.Combine(moduleDir, i))}"));
            string defFlags = string.Join(" ", data.Defines.Select(d => $"-D{d}"));

            foreach (var cpp in cppFiles)
            {
                string objName = Path.Combine("Intermediate", "Build", platform.ToString(), config.ToString(), data.Name, Path.GetFileNameWithoutExtension(cpp) + ".o");
                objName = NormalizePath(objName);

                sb.AppendLine($"build {objName}: cxx {NormalizePath(cpp)}");
                sb.AppendLine($"  includes = {incFlags}");
                sb.AppendLine($"  defines = {defFlags}");

                objectFiles.Add(objName);
            }

            string ext = platform == TargetPlatform.Mac ? ".dylib" : ".so";
            string targetOut = data.Type switch
            {
                "DynamicLibrary" => NormalizePath(Path.Combine("Binaries", platform.ToString(), config.ToString(), $"lib{data.Name}{ext}")),
                "Executable" => NormalizePath(Path.Combine("Binaries", platform.ToString(), config.ToString(), data.Name)),
                _ => NormalizePath(Path.Combine("Binaries", platform.ToString(), config.ToString(), $"lib{data.Name}.a"))
            };

            string ruleName = data.Type == "Executable" ? "link_exe" : "link_so";
            sb.AppendLine($"build {targetOut}: {ruleName} {string.Join(" ", objectFiles)}");
            sb.AppendLine();
            allTargets.Add(targetOut);
        }

        sb.AppendLine($"build all: phony {string.Join(" ", allTargets)}");
        sb.AppendLine("default all");

        File.WriteAllText(ninjaPath, sb.ToString());
        Console.WriteLine($" -> Generated Ninja Build Manifest: {ninjaPath}");
    }

    public static void Execute(string rootDir)
    {
        Console.WriteLine("\n[BasaltBuild] Running Ninja Build...");
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "ninja",
            WorkingDirectory = rootDir,
            UseShellExecute = false
        };
        Process.Start(psi)?.WaitForExit();
    }

    private static List<string> GetSourceFiles(string baseDir, List<string> subDirs, string pattern)
    {
        var files = new List<string>();
        foreach (var subDir in subDirs)
        {
            string fullPath = Path.Combine(baseDir, subDir);
            if (Directory.Exists(fullPath))
            {
                files.AddRange(Directory.GetFiles(fullPath, pattern, SearchOption.AllDirectories));
            }
        }
        return files;
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }
}
