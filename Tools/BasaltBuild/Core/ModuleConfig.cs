using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;

namespace BasaltBuild.Core;

public enum TargetPlatform { Win64, Linux, Mac }
public enum BuildConfiguration { Debug, Release, Development, Shipping }
public enum CommandAction { Configure, Build, Export, Publish, Clean, Help, Unknown }

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
public class ModuleConfig
{
    [YamlMember(Alias = "Module", ApplyNamingConventions = false)]
    public ModuleData Module { get; set; } = new();
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
public class ModuleData
{
    [YamlMember(Alias = "Name", ApplyNamingConventions = false)]
    public string Name { get; set; } = "";

    [YamlMember(Alias = "Type", ApplyNamingConventions = false)]
    public string Type { get; set; } = "StaticLibrary";

    [YamlMember(Alias = "CppVersion", ApplyNamingConventions = false)]
    public string CppVersion { get; set; } = "20";

    [YamlMember(Alias = "Defines", ApplyNamingConventions = false)]
    public List<string> Defines { get; set; } = new();

    [YamlMember(Alias = "IncludeDirectories", ApplyNamingConventions = false)]
    public List<string> IncludeDirectories { get; set; } = new();

    [YamlMember(Alias = "SourceDirectories", ApplyNamingConventions = false)]
    public List<string> SourceDirectories { get; set; } = new();

    [YamlMember(Alias = "Dependencies", ApplyNamingConventions = false)]
    public List<string> Dependencies { get; set; } = new();
}
