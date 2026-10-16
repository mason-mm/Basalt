using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BasaltBuild.CLI;

[Obsolete("BasaltProjectModel is deprecated. This feature will be added back shortly.", true)]
public class BasaltProjectModel
{
    public string Name { get; set; } = string.Empty;
    public string EngineVersion { get; set; } = "1.0.0";
    public string StartupScene { get; set; } = "Content/Scenes/MainScene.bsscene";
}

[Obsolete("ProjectJsonContext is deprecated. This feature will be added back shortly.", true)]
[JsonSerializable(typeof(BasaltProjectModel))]
public partial class ProjectJsonContext : JsonSerializerContext
{
}

[Obsolete("ProjectInitializer is deprecated. This feature will be added back shortly.", true)]
public static class ProjectInitializer
{
    public static void CreateProject(string inputPath, string? targetDirectory = null)
    {
        string projectName;
        string projectRoot;

        // Determine destination folder and project name
        if (!string.IsNullOrWhiteSpace(targetDirectory))
        {
            // Usage: BBT new <ProjectName> <Path>
            projectName = Path.GetFileName(inputPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            projectRoot = Path.GetFullPath(Path.Combine(targetDirectory, inputPath));
        }
        else
        {
            // Usage: BBT new <ProjectNameOrPath>
            projectRoot = Path.GetFullPath(inputPath);
            projectName = Path.GetFileName(projectRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        if (string.IsNullOrWhiteSpace(projectName))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Error] Invalid project name or path specified.");
            Console.ResetColor();
            return;
        }

        if (Directory.Exists(projectRoot) && Directory.GetFileSystemEntries(projectRoot).Length > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Error] Target directory '{projectRoot}' already exists and is not empty.");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[BasaltBuild] Initializing new game project '{projectName}' at:");
        Console.WriteLine($" -> {projectRoot}");
        Console.ResetColor();

        // Standard Cross-Platform Directory Layout
        string[] directories = new[]
        {
            Path.Combine(projectRoot, "Content", "Audio"),
            Path.Combine(projectRoot, "Content", "Materials"),
            Path.Combine(projectRoot, "Content", "Models"),
            Path.Combine(projectRoot, "Content", "Scenes"),
            Path.Combine(projectRoot, "Content", "Textures"),
            Path.Combine(projectRoot, "Source"),
            Path.Combine(projectRoot, "Config")
        };

        foreach (var dir in directories)
        {
            Directory.CreateDirectory(dir);
        }

        // Generate <ProjectName>.basalt descriptor file
        var projectData = new BasaltProjectModel { Name = projectName };

        string jsonContent = JsonSerializer.Serialize(
            projectData,
            ProjectJsonContext.Default.BasaltProjectModel
        );

        File.WriteAllText(Path.Combine(projectRoot, $"{projectName}.basalt"), jsonContent);

        // Generate Default Engine Configuration
        string defaultConfig = Path.Combine(projectRoot, "Config", "DefaultEngine.ini");
        File.WriteAllText(defaultConfig, "; Basalt Engine Configuration\n[Rendering]\nRHI=Vulkan\n");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[Success] Created project '{projectName}'!");
        Console.ResetColor();
    }
}
