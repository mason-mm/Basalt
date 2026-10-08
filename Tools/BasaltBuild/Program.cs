using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using BasaltBuild.CLI;
using BasaltBuild.Core;
using BasaltBuild.Generators;
using BasaltBuild.Pipeline;
using YamlDotNet.RepresentationModel;

namespace BasaltBuild;

class Program
{
    static void Main(string[] args)
    {
        CLIArguments cli = CommandLineParser.Parse(args);

        if (args.Length >= 2 && (args[0].Equals("new", StringComparison.OrdinalIgnoreCase) || args[0].Equals("init", StringComparison.OrdinalIgnoreCase)))
        {
            string inputNameOrPath = args[1];
            string? targetPath = args.Length >= 3 ? args[2] : null;

            ProjectInitializer.CreateProject(inputNameOrPath, targetPath);
            return;
        }

        if (cli.Action == CommandAction.Help || cli.Action == CommandAction.Unknown)
        {
            CommandLineParser.PrintHelp();
            return;
        }

        string rootDir = GetWorkspaceRootDir();
        Console.WriteLine($"[BasaltBuild] Host OS: {RuntimeInformation.OSDescription}");
        Console.WriteLine($"[BasaltBuild] Action: {cli.Action} | Target: {cli.Target} | Config: {cli.Config}");
        Console.WriteLine($"[BasaltBuild] Workspace Root: {rootDir}\n");

        if (cli.Action == CommandAction.Clean)
        {
            ExecuteClean(rootDir);
            return;
        }

        var basaltFiles = Directory.GetFiles(rootDir, "Build.basalt", SearchOption.AllDirectories);
        List<(string ModulePath, ModuleData Data)> modules = new();

        foreach (var file in basaltFiles)
        {
            try
            {
                string yamlText = File.ReadAllText(file);
                ModuleData? moduleData = ParseModuleYaml(yamlText);

                if (moduleData != null && !string.IsNullOrEmpty(moduleData.Name))
                {
                    string moduleDir = Path.GetDirectoryName(file)!;
                    modules.Add((moduleDir, moduleData));
                    Console.WriteLine($"[Found Module] {moduleData.Name} at {Path.GetRelativePath(rootDir, moduleDir)}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[Error] Module missing or has no Name in {file}");
                    Console.ResetColor();
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                string detail = e.InnerException != null ? e.InnerException.Message : e.Message;
                Console.WriteLine($"[Error] Failed parsing {file}: {detail}");
                Console.ResetColor();
            }
        }

        if (modules.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[BasaltBuild] No valid modules found. Exiting.");
            Console.ResetColor();
            return;
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

    // 100% Native AOT Safe YAML Parser (Bypasses Reflection entirely)
    static ModuleData? ParseModuleYaml(string yamlText)
    {
        var yaml = new YamlStream();
        using var reader = new StringReader(yamlText);
        yaml.Load(reader);

        if (yaml.Documents.Count == 0 || yaml.Documents[0].RootNode is not YamlMappingNode root)
            return null;

        var moduleData = new ModuleData();
        YamlMappingNode? moduleNode = null;

        // Locate the "Module:" root key
        foreach (var kvp in root.Children)
        {
            if (kvp.Key is YamlScalarNode key && key.Value == "Module")
            {
                moduleNode = kvp.Value as YamlMappingNode;
                break;
            }
        }

        if (moduleNode == null) return null;

        // Map properties explicitly so the AOT compiler sees the setters being used
        foreach (var kvp in moduleNode.Children)
        {
            if (kvp.Key is YamlScalarNode keyNode && keyNode.Value != null)
            {
                string key = keyNode.Value;
                if (key == "Name" && kvp.Value is YamlScalarNode nameNode)
                    moduleData.Name = nameNode.Value ?? "";
                else if (key == "Type" && kvp.Value is YamlScalarNode typeNode)
                    moduleData.Type = typeNode.Value ?? "StaticLibrary";
                else if (key == "CppVersion" && kvp.Value is YamlScalarNode cppNode)
                    moduleData.CppVersion = cppNode.Value ?? "20";
                else if (key == "Defines" && kvp.Value is YamlSequenceNode definesSeq)
                    foreach (var item in definesSeq.Children) moduleData.Defines.Add(((YamlScalarNode)item).Value ?? "");
                else if (key == "IncludeDirectories" && kvp.Value is YamlSequenceNode incSeq)
                    foreach (var item in incSeq.Children) moduleData.IncludeDirectories.Add(((YamlScalarNode)item).Value ?? "");
                else if (key == "SourceDirectories" && kvp.Value is YamlSequenceNode srcSeq)
                    foreach (var item in srcSeq.Children) moduleData.SourceDirectories.Add(((YamlScalarNode)item).Value ?? "");
                else if (key == "Dependencies" && kvp.Value is YamlSequenceNode depSeq)
                    foreach (var item in depSeq.Children) moduleData.Dependencies.Add(((YamlScalarNode)item).Value ?? "");
            }
        }

        return moduleData;
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
            foreach (var m in modules)
            {
                MSBuildGenerator.GenerateVcxproj(rootDir, m.ModulePath, m.Data, target);
            }
            MSBuildGenerator.GenerateSolution(rootDir, modules);
        }
        else
        {
            NinjaGenerator.GenerateNinjaFile(rootDir, modules, target, config);
        }
    }

    static void ExecuteBuild(string rootDir, TargetPlatform target, BuildConfiguration config)
    {
        if (target == TargetPlatform.Win64)
        {
            MSBuildGenerator.Execute(rootDir, config);
        }
        else
        {
            NinjaGenerator.Execute(rootDir);
        }
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
