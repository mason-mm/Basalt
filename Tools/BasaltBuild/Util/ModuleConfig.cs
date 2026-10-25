using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;

/// <summary>
/// Holds all data classes for YAML parsing.
/// </summary>

namespace BasaltBuild.Util;

/// <summary>
/// All available platforms to build for.
/// </summary>
public enum TargetPlatform { Win64, Linux, Mac }

/// <summary>
/// All available targets to optimize for.
/// </summary>
public enum BuildConfiguration { Debug, Release, Development, Shipping }

/// <summary>
/// Master class that has ModuleData as a member.
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
public class ModuleConfig
{
    /// <summary>
    /// Explicit public parameterless constructor for reflection/deserialization.
    /// </summary>
    public ModuleConfig() { }

    [YamlMember(Alias = "Module", ApplyNamingConventions = false)]
    public ModuleData Module { get; set; } = new();
}

/// <summary>
/// @brief Has all of the members YAML files (.basalt) can have.
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
public class ModuleData
{
    /// <summary>
    /// Explicit public parameterless constructor for reflection/deserialization.
    /// </summary>
    public ModuleData() { }

    [YamlMember(Alias = "Name", ApplyNamingConventions = false)]
    public string Name { get; set; } = "NewModule";

    [YamlMember(Alias = "Type", ApplyNamingConventions = false)]
    public string Type { get; set; } = "StaticLibrary";

    [YamlMember(Alias = "CppVersion", ApplyNamingConventions = false)]
    public string CppVersion { get; set; } = "20";

    [YamlMember(Alias = "Platforms", ApplyNamingConventions = false)]
    public List<string> Platforms { get; set; } = new();

    [YamlMember(Alias = "Defines", ApplyNamingConventions = false)]
    public List<string> Defines { get; set; } = new();

    // Looks recursively.
    [YamlMember(Alias = "IncludeDirectories", ApplyNamingConventions = false)]
    public List<string> IncludeDirectories { get; set; } = new();

    // Looks recursively.
    [YamlMember(Alias = "SourceDirectories", ApplyNamingConventions = false)]
    public List<string> SourceDirectories { get; set; } = new();

    // Other modules names.
    [YamlMember(Alias = "Dependencies", ApplyNamingConventions = false)]
    public List<string> Dependencies { get; set; } = new();
}
