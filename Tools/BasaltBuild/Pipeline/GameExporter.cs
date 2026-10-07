using BasaltBuild.Core;

namespace BasaltBuild.Pipeline;

public static class GameExporter
{
    public static void ExportGamePackage(string rootDir, List<(string ModulePath, ModuleData Data)> modules, TargetPlatform target, BuildConfiguration config)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"\n[BasaltBuild Exporter] Exporting Game Build for Gamers ({target} - {config})...");
        Console.ResetColor();

        string exportDir = Path.Combine(rootDir, "Export", $"{target}_{config}");
        if (Directory.Exists(exportDir)) Directory.Delete(exportDir, true);
        Directory.CreateDirectory(exportDir);

        string binariesPath = Path.Combine(rootDir, "Binaries", target.ToString(), config.ToString());

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

        // Copy Content folder if present
        string contentDir = Path.Combine(rootDir, "Content");
        if (Directory.Exists(contentDir))
        {
            CopyDirectory(contentDir, Path.Combine(exportDir, "Content"));
            Console.WriteLine(" -> Packaged Content/Assets Directory");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[BasaltBuild] Standalone Game Export complete!\nOutput directory: {exportDir}");
        Console.ResetColor();
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
        }
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            CopyDirectory(subDir, Path.Combine(destinationDir, Path.GetFileName(subDir)));
        }
    }
}
