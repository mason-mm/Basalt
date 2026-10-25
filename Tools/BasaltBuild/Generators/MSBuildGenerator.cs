using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BasaltBuild.Util;

/// <summary>
/// Generates the required files to build using MSBuild.
/// </summary>
namespace BasaltBuild.Generators;

/// <summary>
/// Generates MSBuild files for building the project.
/// </summary>
public class MSBuildGenerator : BuildGenerator
{
    /// <inheritdoc />
    public override void ExecuteBuild(string rootDir, BuildConfiguration? config = null)
    {
        // Get the config.
        BuildConfiguration activeConfig = config ?? BuildConfiguration.Debug;
        Console.WriteLine($"\n[BasaltBuild] Triggering MSBuild ({activeConfig}|x64).");

        // Setup the process.
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c msbuild Basalt.sln /p:Configuration={activeConfig} /p:Platform=x64 /m",
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
        Console.WriteLine("[BasaltBuild] Configuring MSBuild solution and projects...");

        // Setup binaries directories.
        string projectFilesDir = Path.Combine(rootDir, "Intermediate", "ProjectFiles");
        Directory.CreateDirectory(projectFilesDir);

        // Filter modules based on target platform compatibility
        var activeModules = modules.Where(m =>
            m.Data.Platforms == null ||
            m.Data.Platforms.Count == 0 ||
            m.Data.Platforms.Contains(target.ToString(), StringComparer.OrdinalIgnoreCase)
        ).ToList();

        // Generate .vcxproj files for active modules.
        foreach (var (moduleDir, module) in activeModules)
        {
            GenerateVcxproj(rootDir, moduleDir, module, activeModules, target);
        }

        // Generate the master solution file linking active modules.
        GenerateSolution(rootDir, activeModules);
    }

    /// <summary>
    /// Generates the Vcxproj files for each module.
    /// </summary>
    /// <param name="rootDir">The directory to start the build form (CWD).</param>
    /// <param name="moduleDir">The directory the targeted module is in.</param>
    /// <param name="module">The modules data object.</param>
    /// <param name="allModules">The full list of active workspace modules for dependency lookups.</param>
    /// <param name="target">The target platform to build for.</param>
    private static void GenerateVcxproj(string rootDir, string moduleDir, ModuleData module, List<(string ModulePath, ModuleData Data)> allModules, TargetPlatform target)
    {
        // Setup file paths.
        string projectFilesDir = Path.Combine(rootDir, "Intermediate", "ProjectFiles");
        string vcxprojPath = Path.Combine(projectFilesDir, $"{module.Name}.vcxproj");

        // Get files using the helper method in the base class.
        var cppFiles = GetFiles(moduleDir, module.SourceDirectories, "*.cpp")
            .Concat(GetFiles(moduleDir, module.SourceDirectories, "*.c")).ToList();
        var hFiles = GetFiles(moduleDir, module.SourceDirectories, "*.h")
            .Concat(GetFiles(moduleDir, module.IncludeDirectories, "*.h")).ToList();

        // Find config type.
        string configType = module.Type switch
        {
            "DynamicLibrary" => "DynamicLibrary",
            "Executable" => "Application",
            _ => "StaticLibrary"
        };

        // Collect module's own include directories
        var includeDirs = new List<string>();
        foreach (var d in module.IncludeDirectories ?? [])
        {
            includeDirs.Add(Path.Combine(moduleDir, d));
        }

        // Filter dependencies to only include active modules
        var validDeps = (module.Dependencies ?? []).Where(dep => allModules.Any(m => m.Data.Name == dep)).ToList();

        // Collect include directories from dependent modules
        foreach (var depName in validDeps)
        {
            var depModule = allModules.FirstOrDefault(m => m.Data.Name == depName);
            if (depModule.Data != null)
            {
                foreach (var depInc in depModule.Data.IncludeDirectories ?? [])
                {
                    includeDirs.Add(Path.Combine(depModule.ModulePath, depInc));
                }
            }
        }

        // Setup the XDocument.
        string projectGuid = GenerateDeterministicGuid(module.Name).ToString().ToUpper();
        XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

        var doc = new XDocument(
            new XElement(ns + "Project",
                new XAttribute("DefaultTargets", "Build"),
                new XAttribute("ToolsVersion", "Current"),

                new XElement(ns + "ItemGroup",
                    new XAttribute("Label", "ProjectConfigurations"),
                    Enum.GetNames<BuildConfiguration>().Select(cfg => CreateProjectConfig(ns, $"{cfg}|x64"))
                ),

                new XElement(ns + "PropertyGroup",
                    new XAttribute("Label", "Globals"),
                    new XElement(ns + "ProjectGuid", $"{{{projectGuid}}}"),
                    new XElement(ns + "Keyword", "Win32Proj"),
                    new XElement(ns + "RootNamespace", module.Name),
                    new XElement(ns + "WindowsTargetPlatformVersion", "10.0")
                ),

                new XElement(ns + "Import", new XAttribute("Project", @"$(VCTargetsPath)\Microsoft.Cpp.Default.props")),

                new XElement(ns + "PropertyGroup",
                    new XElement(ns + "ConfigurationType", configType),
                    new XElement(ns + "PlatformToolset", "$(DefaultPlatformToolset)")
                ),

                new XElement(ns + "Import", new XAttribute("Project", @"$(VCTargetsPath)\Microsoft.Cpp.props")),

                new XElement(ns + "PropertyGroup",
                    new XElement(ns + "OutDir", @"$(SolutionDir)Binaries\" + target + @"\$(Configuration)\"),
                    new XElement(ns + "IntDir", @"$(SolutionDir)Intermediate\Build\" + target + @"\$(Configuration)\" + module.Name + @"\")
                ),

                new XElement(ns + "ItemDefinitionGroup",
                    new XElement(ns + "ClCompile",
                        new XElement(ns + "WarningLevel", "Level3"),
                        new XElement(ns + "LanguageStandard", $"stdcpp{module.CppVersion}"),
                        new XElement(ns + "AdditionalIncludeDirectories", string.Join(";", includeDirs)),
                        new XElement(ns + "PreprocessorDefinitions", string.Join(";", module.Defines ?? []))
                    )
                ),

                new XElement(ns + "ItemGroup", hFiles.Select(h => new XElement(ns + "ClInclude", new XAttribute("Include", h)))),
                new XElement(ns + "ItemGroup", cppFiles.Select(c => new XElement(ns + "ClCompile", new XAttribute("Include", c)))),

                new XElement(ns + "ItemGroup", validDeps.Select(dep =>
                    new XElement(ns + "ProjectReference", new XAttribute("Include", $"{dep}.vcxproj"),
                        new XElement(ns + "Project", $"{{{GenerateDeterministicGuid(dep).ToString().ToUpper()}}}")
                    )
                )),

                new XElement(ns + "Import", new XAttribute("Project", @"$(VCTargetsPath)\Microsoft.Cpp.targets"))
            )
        );

        // Clean up.
        doc.Save(vcxprojPath);
        Console.WriteLine($" -> Generated MSBuild Project: {Path.GetFileName(vcxprojPath)}");
    }

    /// <summary>
    /// Generates the Solution files.
    /// </summary>
    /// <param name="rootDir">The directory to start the build form (CWD).</param>
    /// <param name="modules">The list of active modules to include in solution.</param>
    private static void GenerateSolution(string rootDir, List<(string ModulePath, ModuleData Data)> modules)
    {
        string slnPath = Path.Combine(rootDir, "Basalt.sln");
        StringBuilder sb = new();

        // Write the contents of the file.
        sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
        sb.AppendLine("# Visual Studio Version 17");

        // Add the modules.
        foreach (var m in modules)
        {
            string projGuid = GenerateDeterministicGuid(m.Data.Name).ToString().ToUpper();
            string vcxprojPath = Path.Combine(rootDir, "Intermediate", "ProjectFiles", $"{m.Data.Name}.vcxproj");
            string relPath = Path.GetRelativePath(rootDir, vcxprojPath);

            sb.AppendLine($"Project(\"{{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}}\") = \"{m.Data.Name}\", \"{relPath}\", \"{{{projGuid}}}\"");
            sb.AppendLine("EndProject");
        }

        sb.AppendLine("Global");

        sb.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
        var configNames = Enum.GetNames<BuildConfiguration>();
        foreach (var cfg in configNames)
        {
            sb.AppendLine($"\t\t{cfg}|x64 = {cfg}|x64");
        }
        sb.AppendLine("\tEndGlobalSection");

        sb.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
        foreach (var m in modules)
        {
            string projGuid = GenerateDeterministicGuid(m.Data.Name).ToString().ToUpper();
            foreach (var cfg in configNames)
            {
                sb.AppendLine($"\t\t{{{projGuid}}}.{cfg}|x64.ActiveCfg = {cfg}|x64");
                sb.AppendLine($"\t\t{{{projGuid}}}.{cfg}|x64.Build.0 = {cfg}|x64");
            }
        }
        sb.AppendLine("\tEndGlobalSection");

        sb.AppendLine("EndGlobal");

        // Clean up.
        File.WriteAllText(slnPath, sb.ToString());
        Console.WriteLine($" -> Generated Visual Studio Solution: {Path.GetFileName(slnPath)}");
    }

    /// <summary>
    /// Creates the ProjectConfiguration Element for each section.
    /// </summary>
    /// <param name="ns">The XNamepsace to use.</param>
    /// <param name="name">The name of the configuration.</param>
    /// <returns>The generated XElement.</returns>
    private static XElement CreateProjectConfig(XNamespace ns, string name)
    {
        return new XElement(ns + "ProjectConfiguration",
            new XAttribute("Include", name),
            new XElement(ns + "Configuration", name.Split('|')[0]),
            new XElement(ns + "Platform", name.Split('|')[1])
        );
    }
}
