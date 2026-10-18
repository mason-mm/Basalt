using System;
using System.Collections.Generic;
using System.IO;
using BasaltBuild.Generators;
using BasaltBuild.Pipelines;
using BasaltBuild.Util;
using YamlDotNet.Serialization;

/// <summary>
/// Holds all of the methods that CommandLineParser will call based on the CLI commands.
/// </summary>

namespace BasaltBuild.CLI;

/// <summary>
/// This class has all methods CommandLineParser will call.
/// </summary>
public static class CommandLineHelper
{
    /// <summary>
    /// Finds and parses all .basalt module files.
    /// </summary>
    /// <param name="rootDir">The root dir to start looking in.</param>
    /// <returns>A list of all found modules.</returns>
    public static List<(string ModulePath, ModuleData Data)> LoadModules(string rootDir)
    {
        var modules = new List<(string, ModuleData)>();
        string[] basaltFiles = Directory.GetFiles(rootDir, "*.basalt", SearchOption.AllDirectories);

        // Use the static builder with your context
        var deserializer = new StaticDeserializerBuilder(new BasaltYamlContext())
            .Build();

        foreach (var file in basaltFiles)
        {
            try
            {
                string content = File.ReadAllText(file);
                var config = deserializer.Deserialize<ModuleConfig>(content);
                if (config?.Module != null)
                {
                    string moduleDir = Path.GetDirectoryName(file) ?? rootDir;
                    modules.Add((moduleDir, config.Module));
                    Console.WriteLine($" -> Loaded Module: {config.Module.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Error] Failed to parse {Path.GetFileName(file)}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  -> Inner: {ex.InnerException.Message}");
                }
                Console.ResetColor();
            }
        }

        return modules;
    }

    /// <summary>
    /// Configures the workspace using the specified backend generator.
    /// </summary>
    /// <param name="rootDir">The root directory (CWD).</param>
    /// <param name="backend">The backend generator to use.</param>
    /// <param name="target">The target platform to configure for.</param>
    public static void Configure(string rootDir, string backend, TargetPlatform target)
    {
        var modules = LoadModules(rootDir);
        BuildGenerator generator = GetGenerator(backend);

        Console.WriteLine($"\n[BasaltBuild] Configuring project using [{backend}] for [{target}]...");
        generator.Configure(rootDir, modules, target);
    }

    /// <summary>
    /// Builds the project using the specified backend generator.
    /// </summary>
    /// <param name="rootDir">The root directory (CWD).</param>
    /// <param name="backend">The backend generator to use.</param>
    /// <param name="config">The build configuration to build for.</param>
    public static void Build(string rootDir, string backend, BuildConfiguration config)
    {
        BuildGenerator generator = GetGenerator(backend);
        generator.ExecuteBuild(rootDir, config);
    }

    /// <summary>
    /// Cleans all build files from the project.
    /// </summary>
    /// <param name="rootDir">The root directory (CWD).</param>
    public static void Clean(string rootDir)
    {
        Console.WriteLine("\n[BasaltBuild] Cleaning workspace...");

        string[] dirsToClean = { "Intermediate", "Binaries", "Publish", "Export" }; // If you are building from source, you can add more things here to clean.
        foreach (var dir in dirsToClean)
        {
            string fullPath = Path.Combine(rootDir, dir);
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
                Console.WriteLine($" -> Deleted: {dir}/");
            }
        }

        // Clean generated solution or Ninja files.
        foreach (var file in Directory.GetFiles(rootDir, "build_*.ninja"))
        {
            File.Delete(file);
            Console.WriteLine($" -> Deleted: {Path.GetFileName(file)}");
        }

        string slnFile = Path.Combine(rootDir, "Basalt.sln");
        if (File.Exists(slnFile))
        {
            File.Delete(slnFile);
            Console.WriteLine($" -> Deleted: Basalt.sln");
        }

        // Clean up.
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[BasaltBuild] Clean complete.");
        Console.ResetColor();
    }

    /// <summary>
    /// Publishes the engine SDK
    /// </summary>
    /// <param name="rootDir">The root dir (CWD)</param>
    /// <param name="target">The target platform to publish for.</param>
    /// <param name="config">The build configuration to use, Shipping is recommended.</param>
    public static void Publish(string rootDir, TargetPlatform target, BuildConfiguration config)
    {
        var modules = LoadModules(rootDir);
        var context = new PipelineContext { RootDir = rootDir, Modules = modules, Target = target, Config = config };
        EnginePublisher.Execute(context);
    }

    /// <summary>
    /// Exports a game.
    /// </summary>
    /// <param name="rootDir">The root dir (CWD)</param>
    /// <param name="target">The target platform to export for.</param>
    /// <param name="config">The build configuration to use, Shipping is recommended.</param>
    public static void Export(string rootDir, TargetPlatform target, BuildConfiguration config)
    {
        var modules = LoadModules(rootDir);
        var context = new PipelineContext { RootDir = rootDir, Modules = modules, Target = target, Config = config };
        GameExporter.Execute(context);
    }

    /// <summary>
    /// Prints the help message to the console.
    /// </summary>
    public static void PrintHelp()
    {
        Console.WriteLine("=== BasaltBuild CLI Help ===");
        Console.WriteLine("Usage: basaltbuild <command> [options]");
        Console.WriteLine("\nCommands:");
        Console.WriteLine("  configure    Generate project and workspace files");
        Console.WriteLine("  build        Compile the project binaries");
        Console.WriteLine("  clean        Remove intermediate files and binaries");
        Console.WriteLine("  publish      Package the Engine SDK");
        Console.WriteLine("  export       Export the final game package for distribution");
        Console.WriteLine("\nOptions:");
        Console.WriteLine("  --backend    Build system backend: msbuild | ninja (default: msbuild)");
        Console.WriteLine("  --platform   Target platform: Win64 | Linux | Mac (default: Win64)");
        Console.WriteLine("  --config     Build configuration: Debug | Release | Development | Shipping (default: Debug)");
    }

    /// <summary>
    /// Gets the generator instance based on the string input.
    /// </summary>
    /// <param name="backend">The string input to compare.</param>
    /// <returns>The GeneratorInstance.</returns>
    private static BuildGenerator GetGenerator(string backend)
    {
        return backend.ToLowerInvariant() switch
        {
            "ninja" => new NinjaGenerator(),
            _ => new MSBuildGenerator()
        };
    }
}
