using YamlDotNet.Serialization;

namespace BasaltBuild.Core;

// Instructs YamlDotNet source generator to produce reflection-free parsers for AOT
[YamlStaticContext]
public partial class BasaltYamlContext : StaticContext
{
}