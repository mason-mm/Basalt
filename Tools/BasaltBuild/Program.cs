using System.Runtime.InteropServices;
using BasaltBuild.CLI;
using BasaltBuild.Core;
using BasaltBuild.Generators;
using BasaltBuild.Pipeline;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace BasaltBuild;

class Program
{
    static void Main(string[] args)
    {
        CLIArguments cli = CommandLineParser.Parse(args);

        // 1. Handle Project Initializer ('BBT new <ProjectName> [Path]')
        if (args.Length >= 2 && (args[0].Equals("new", StringComparison.OrdinalIgnoreCase) || args[0].Equals("init", StringComparison.OrdinalIgnoreCase)))
        {
            string inputNameOrPath = args[1];
            string? targetPath = args.Length >= 3 ? args[2] : null;

            ProjectInitializer.CreateProject(inputNameOrPath, targetPath);
            return;
        }

        // 2. Handle Help / Unknown Commands
        if (cli.Action == CommandAction.Help || cli.Action == CommandAction.Unknown)
        {
            CommandLineParser.PrintHelp();
            return;
        }

        string rootDir = GetWorkspaceRootDir();
        Console.WriteLine($"[BasaltBuild] Host OS: {RuntimeInformation.OSDescription}");
        Console.WriteLine($"[BasaltBuild] Action: {cli.Action} | Target: {cli.Target} | Config: {cli.Config}");
        Console.WriteLine($"[BasaltBuild] Workspace Root: {rootDir}\n");

        // 3. Handle Clean Command
        if (cli.Action == CommandAction.Clean)
        {
            ExecuteClean(rootDir);
            return;
        }

        // 4. YamlDeserializer with safe property matching & fallback list initialization
        // Use StaticDeserializerBuilder instead of DeserializerBuilder for Native AOT compliance
        var deserializer = new StaticDeserializerBuilder(new BasaltYamlContext())
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var basaltFiles = Directory.GetFiles(rootDir, "Build.basalt", SearchOption.AllDirectories);
        List<(string ModulePath, ModuleData Data)> modules = new();

        foreach (var file in basaltFiles)
        {
            try
            {
                string yamlText = File.ReadAllText(file);
                var moduleConfig = deserializer.Deserialize<ModuleConfig>(yamlText);

                if (moduleConfig?.Module != null)
                {
                    moduleConfig.Module.Defines ??= new();
                    moduleConfig.Module.IncludeDirectories ??= new();
                    moduleConfig.Module.SourceDirectories ??= new();
                    moduleConfig.Module.Dependencies ??= new();

                    string moduleDir = Path.GetDirectoryName(file)!;
                    modules.Add((moduleDir, moduleConfig.Module));
                    Console.WriteLine($"[Found Module] {moduleConfig.Module.Name} at {Path.GetRelativePath(rootDir, moduleDir)}");
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Error] Failed parsing {file}: {e.Message}");
                Console.ResetColor();
            }
        }

        switch (cli.Action)
        {
            case CommandAction.Configure:
                ConfigureWorkspace(rootDir, modules, cli.Target, cli.Config);
                break;

            case CommandAction.Build:
                ConfigureWorkspace(rootDir, modules, cli.Target, cli.Config);
                ExecuteBuild(rootDir, cli.Target, cli.Config);
                break;

            case CommandAction.Export:
                ConfigureWorkspace(rootDir, modules, cli.Target, cli.Config);
                ExecuteBuild(rootDir, cli.Target, cli.Config);
                GameExporter.ExportGamePackage(rootDir, modules, cli.Target, cli.Config);
                break;

            case CommandAction.Publish:
                ConfigureWorkspace(rootDir, modules, cli.Target, cli.Config);
                ExecuteBuild(rootDir, cli.Target, cli.Config);
                EnginePublisher.PublishSDK(rootDir, modules, cli.Target, cli.Config);
                break;
        }
    }

    static void ExecuteClean(string rootDir)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[BasaltBuild] Cleaning workspace build artifacts...");
        Console.ResetColor();

        string[] dirsToClean = new[]
        {
            Path.Combine(rootDir, "Binaries"),
            Path.Combine(rootDir, "Intermediate"),
            Path.Combine(rootDir, "Export"),
            Path.Combine(rootDir, "Publish")
        };

        foreach (var dir in dirsToClean)
        {
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                    Console.WriteLine($" -> Deleted: {Path.GetRelativePath(rootDir, dir)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" -> Failed to delete {dir}: {ex.Message}");
                }
            }
        }

        string slnFile = Path.Combine(rootDir, "Basalt.sln");
        if (File.Exists(slnFile))
        {
            File.Delete(slnFile);
            Console.WriteLine(" -> Deleted: Basalt.sln");
        }

        string ninjaFile = Path.Combine(rootDir, "build.ninja");
        if (File.Exists(ninjaFile))
        {
            File.Delete(ninjaFile);
            Console.WriteLine(" -> Deleted: build.ninja");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n[BasaltBuild] Clean complete.");
        Console.ResetColor();
    }

    static void ConfigureWorkspace(string rootDir, List<(string ModulePath, ModuleData Data)> modules, TargetPlatform target, BuildConfiguration config)
    {
        if (target == TargetPlatform.Win64)
        {
            foreach (var m in modules) MSBuildGenerator.GenerateVcxproj(rootDir, m.ModulePath, m.Data, target);
            MSBuildGenerator.GenerateSolution(rootDir, modules);
        }
        else
        {
            NinjaGenerator.GenerateNinjaFile(rootDir, modules, target, config);
        }
    }

    static void ExecuteBuild(string rootDir, TargetPlatform target, BuildConfiguration config)
    {
        if (target == TargetPlatform.Win64) MSBuildGenerator.Execute(rootDir, config);
        else NinjaGenerator.Execute(rootDir);
    }

    static string GetWorkspaceRootDir()
    {
        string currentDir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(currentDir))
        {
            if (Directory.Exists(Path.Combine(currentDir, "Modules")) && Directory.Exists(Path.Combine(currentDir, "Tools")))
            {
                return currentDir;
            }
            var parent = Directory.GetParent(currentDir);
            if (parent == null) break;
            currentDir = parent.FullName;
        }
        return Directory.GetCurrentDirectory();
    }
}
