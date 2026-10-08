using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using BasaltBuild.Core;

namespace BasaltBuild.Generators;

public static class MSBuildGenerator
{
    // Generates a consistent GUID based on the module name
    private static Guid GenerateDeterministicGuid(string input)
    {
        using var md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }

    public static void GenerateVcxproj(string rootDir, string moduleDir, ModuleData module, TargetPlatform target)
    {
        string projectFilesDir = Path.Combine(rootDir, "Intermediate", "ProjectFiles");
        Directory.CreateDirectory(projectFilesDir);
        string vcxprojPath = Path.Combine(projectFilesDir, $"{module.Name}.vcxproj");

        var cppFiles = GetSourceFiles(moduleDir, module.SourceDirectories, "*.cpp");
        var hppFiles = GetSourceFiles(moduleDir, module.SourceDirectories, "*.h")
            .Concat(GetSourceFiles(moduleDir, module.IncludeDirectories, "*.h")).ToList();

        string configType = module.Type switch
        {
            "DynamicLibrary" => "DynamicLibrary",
            "Executable" => "Application",
            _ => "StaticLibrary"
        };

        string projectGuid = GenerateDeterministicGuid(module.Name).ToString().ToUpper();
        XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

        var doc = new XDocument(
            new XElement(ns + "Project",
                new XAttribute("DefaultTargets", "Build"),
                new XAttribute("ToolsVersion", "Current"),

                new XElement(ns + "ItemGroup",
                    new XAttribute("Label", "ProjectConfigurations"),
                    CreateProjectConfig(ns, "Debug|x64"),
                    CreateProjectConfig(ns, "Release|x64"),
                    CreateProjectConfig(ns, "Development|x64"),
                    CreateProjectConfig(ns, "Shipping|x64")
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
                        new XElement(ns + "AdditionalIncludeDirectories", string.Join(";", module.IncludeDirectories.Select(d => Path.Combine(moduleDir, d)))),
                        new XElement(ns + "PreprocessorDefinitions", string.Join(";", module.Defines))
                    )
                ),

                new XElement(ns + "ItemGroup", hppFiles.Select(h => new XElement(ns + "ClInclude", new XAttribute("Include", h)))),
                new XElement(ns + "ItemGroup", cppFiles.Select(c => new XElement(ns + "ClCompile", new XAttribute("Include", c)))),

                new XElement(ns + "ItemGroup", module.Dependencies.Select(dep =>
                    new XElement(ns + "ProjectReference", new XAttribute("Include", $"{dep}.vcxproj"),
                        new XElement(ns + "Project", $"{{{GenerateDeterministicGuid(dep).ToString().ToUpper()}}}")
                    )
                )),

                new XElement(ns + "Import", new XAttribute("Project", @"$(VCTargetsPath)\Microsoft.Cpp.targets"))
            )
        );

        doc.Save(vcxprojPath);
        Console.WriteLine($" -> Generated MSBuild Project: {Path.GetFileName(vcxprojPath)}");
    }

    public static void GenerateSolution(string rootDir, List<(string ModulePath, ModuleData Data)> modules)
    {
        string slnPath = Path.Combine(rootDir, "Basalt.sln");
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
        sb.AppendLine("# Visual Studio Version 17");

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
        string[] configs = { "Debug|x64", "Release|x64", "Development|x64", "Shipping|x64" };
        foreach (var cfg in configs)
        {
            sb.AppendLine($"\t\t{cfg} = {cfg}");
        }
        sb.AppendLine("\tEndGlobalSection");

        // THIS is what tells MSBuild to actually build the mapped projects
        sb.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
        foreach (var m in modules)
        {
            string projGuid = GenerateDeterministicGuid(m.Data.Name).ToString().ToUpper();
            foreach (var cfg in configs)
            {
                sb.AppendLine($"\t\t{{{projGuid}}}.{cfg}.ActiveCfg = {cfg}");
                sb.AppendLine($"\t\t{{{projGuid}}}.{cfg}.Build.0 = {cfg}");
            }
        }
        sb.AppendLine("\tEndGlobalSection");

        sb.AppendLine("EndGlobal");

        File.WriteAllText(slnPath, sb.ToString());
        Console.WriteLine($" -> Generated Visual Studio Solution: {Path.GetFileName(slnPath)}");
    }

    public static void Execute(string rootDir, BuildConfiguration config)
    {
        Console.WriteLine($"\n[BasaltBuild] Triggering MSBuild ({config}|x64)...");
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c msbuild Basalt.sln /p:Configuration={config} /p:Platform=x64 /m",
            WorkingDirectory = rootDir,
            UseShellExecute = false
        };
        Process.Start(psi)?.WaitForExit();
    }

    private static XElement CreateProjectConfig(XNamespace ns, string name)
    {
        return new XElement(ns + "ProjectConfiguration",
            new XAttribute("Include", name),
            new XElement(ns + "Configuration", name.Split('|')[0]),
            new XElement(ns + "Platform", name.Split('|')[1])
        );
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
}
