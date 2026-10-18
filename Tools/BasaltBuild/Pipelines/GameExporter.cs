using System;
using System.Collections.Generic;
using System.IO;
using BasaltBuild.Util;
using BasaltBuild.Pipelines;

/// <summary>
/// Holds the GameExporter class.
/// </summary>
namespace BasaltBuild.Pipelines;

/// <summary>
/// Handles publishing the engine SDK for shipping.
/// </summary>
public static class GameExporter
{
    /// <summary>
    /// Executes the game export pipeline.
    /// </summary>
    public static void Execute(PipelineContext context)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"\n[BasaltBuild Exporter] Exporting Game Build for Gamers ({context.Target} - {context.Config})...");
        Console.ResetColor();

        string exportDir = Path.Combine(context.RootDir, "Export", $"{context.Target}_{context.Config}");
        if (Directory.Exists(exportDir)) Directory.Delete(exportDir, true);
        Directory.CreateDirectory(exportDir);

        string binariesPath = Path.Combine(context.RootDir, "Binaries", context.Target.ToString(), context.Config.ToString());

        if (!Directory.Exists(binariesPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Error] No compiled binaries found at {binariesPath}. Run 'build' first.");
            Console.ResetColor();
            return;
        }

        // Copy Executable and required Runtime DLLs/Assets
        foreach (var file in Directory.GetFiles(binariesPath))
        {
            string ext = Path.GetExtension(file).ToLower();
            // Exclude build artifacts (.lib, .pdb, .exp, .o, .a)
            if (ext is ".exe" or ".dll" or ".so" or ".dylib" or "")
            {
                string dest = Path.Combine(exportDir, Path.GetFileName(file));
                File.Copy(file, dest, true);
                Console.WriteLine($" -> Packaged Release Artifact: {Path.GetFileName(file)}");
            }
        }

        // Content folder
        string contentDir = Path.Combine(context.RootDir, "Content");
        if (Directory.Exists(contentDir))
        {
            PipelineBase.CopyDirectory(contentDir, Path.Combine(exportDir, "Content"));
            Console.WriteLine(" -> Packaged Content/Assets Directory");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[BasaltBuild] Standalone Game Export complete!\nOutput directory: {exportDir}");
        Console.ResetColor();
    }
}
