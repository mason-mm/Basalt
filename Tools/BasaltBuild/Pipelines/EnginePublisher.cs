using System;
using System.Collections.Generic;
using System.IO;
using BasaltBuild.Util;
using BasaltBuild.Pipelines;

/// <summary>
/// Holds the EnginePublisher class.
/// </summary>
namespace BasaltBuild.Pipelines;

/// <summary>
/// Handles publishing the engine SDK for shipping.
/// </summary>
public static class EnginePublisher
{
    /// <summary>
    /// Executes the publishing pipeline.
    /// </summary>
    public static void Execute(PipelineContext context)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n[BasaltBuild Publisher] Publishing Engine SDK ({context.Target} - {context.Config})...");
        Console.ResetColor();

        // Directories.
        string publishDir = Path.Combine(context.RootDir, "Publish", "BasaltEngineSDK");
        string publishBinaries = Path.Combine(publishDir, "Binaries", context.Target.ToString(), context.Config.ToString());
        string publishIncludes = Path.Combine(publishDir, "Include");

        if (Directory.Exists(publishDir)) Directory.Delete(publishDir, true);

        Directory.CreateDirectory(publishBinaries);
        Directory.CreateDirectory(publishIncludes);

        // Copy Engine Binaries.
        string sourceBinaries = Path.Combine(context.RootDir, "Binaries", context.Target.ToString(), context.Config.ToString());
        if (Directory.Exists(sourceBinaries))
        {
            foreach (var file in Directory.GetFiles(sourceBinaries, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(file, Path.Combine(publishBinaries, Path.GetFileName(file)), true);
            }
            Console.WriteLine($" -> Packaged Engine Binaries into: {publishBinaries}");
        }

        // Copy Public Headers.
        foreach (var (moduleDir, data) in context.Modules)
        {
            foreach (var incDir in data.IncludeDirectories)
            {
                string fullInc = Path.Combine(moduleDir, incDir);
                if (Directory.Exists(fullInc))
                {
                    PipelineBase.CopyDirectory(fullInc, publishIncludes);
                }
            }
        }
        Console.WriteLine($" -> Packaged Engine Headers into: {publishIncludes}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[BasaltBuild] Engine SDK successfully published to:\n{publishDir}");
        Console.ResetColor();
    }
}
