using BasaltBuild.Core;
using YamlDotNet.Serialization;

namespace BasaltBuild.Core;

[YamlStaticContext]
[YamlSerializable(typeof(ModuleConfig))]
[YamlSerializable(typeof(ModuleData))]
public partial class BasaltYamlContext : StaticContext
{
}
