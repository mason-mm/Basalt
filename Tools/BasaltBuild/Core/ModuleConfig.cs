namespace BasaltBuild.Core;

public enum TargetPlatform { Win64, Linux, Mac }
public enum BuildConfiguration { Debug, Release, Development, Shipping }
public enum CommandAction { Configure, Build, Export, Publish, Clean, Help, Unknown }

public class ModuleConfig
{
    public ModuleData Module { get; set; } = new();
}

public class ModuleData
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "StaticLibrary"; // StaticLibrary, DynamicLibrary, Executable
    public string CppVersion { get; set; } = "20";
    public List<string> Defines { get; set; } = new();
    public List<string> IncludeDirectories { get; set; } = new();
    public List<string> SourceDirectories { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
}
