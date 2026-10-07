using BasaltBuild.Core;

namespace BasaltBuild.Pipeline;

public static class EnginePublisher
{
    public static void PublishSDK(string rootDir, List<(string ModulePath, ModuleData Data)> modules, TargetPlatform target, BuildConfiguration config)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n[BasaltBuild Publisher] Publishing Engine SDK ({target} - {config})...");
        Console.ResetColor();

        string publishDir = Path.Combine(rootDir, "Publish", "BasaltEngineSDK");
        string publishBinaries = Path.Combine(publishDir, "Binaries", target.ToString(), config.ToString());
        string publishIncludes = Path.Combine(publishDir, "Include");

        if (Directory.Exists(publishDir)) Directory.Delete(publishDir, true);

        Directory.CreateDirectory(publishBinaries);
        Directory.CreateDirectory(publishIncludes);

        // Copy Engine Binaries
        string sourceBinaries = Path.Combine(rootDir, "Binaries", target.ToString(), config.ToString());
        if (Directory.Exists(sourceBinaries))
        {
            foreach (var file in Directory.GetFiles(sourceBinaries, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(file, Path.Combine(publishBinaries, Path.GetFileName(file)), true);
            }
            Console.WriteLine($" -> Packaged Engine Binaries into: {publishBinaries}");
        }

        // Copy Public Headers
        foreach (var (moduleDir, data) in modules)
        {
            foreach (var incDir in data.IncludeDirectories)
            {
                string fullInc = Path.Combine(moduleDir, incDir);
                if (Directory.Exists(fullInc))
                {
                    CopyDirectory(fullInc, publishIncludes);
                }
            }
        }
        Console.WriteLine($" -> Packaged Engine Headers into: {publishIncludes}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[BasaltBuild] Engine SDK successfully published to:\n{publishDir}");
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
