using BasaltBuild.Util;
using YamlDotNet.Serialization;

/// <summary>
/// Required for the YAML parser to work right.
/// </summary>

namespace BasaltBuild.Util;

/// <summary>
/// Manadory context for YAMLDotNet to work.
/// </summary>
[YamlStaticContext]
[YamlSerializable(typeof(ModuleConfig))]
[YamlSerializable(typeof(ModuleData))]
public partial class BasaltYamlContext : StaticContext
{
}
