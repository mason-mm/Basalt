using System.Runtime.InteropServices;
using BasaltBuild.Core;

namespace BasaltBuild.CLI;

public class CLIArguments
{
    public CommandAction Action { get; set; } = CommandAction.Unknown;
    public TargetPlatform Target { get; set; } = GetDefaultPlatform();
    public BuildConfiguration Config { get; set; } = BuildConfiguration.Debug;

    private static TargetPlatform GetDefaultPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return TargetPlatform.Win64;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return TargetPlatform.Mac;
        return TargetPlatform.Linux;
    }
}

public static class CommandLineParser
{
    public static CLIArguments Parse(string[] args)
    {
        var result = new CLIArguments();

        if (args.Length == 0)
        {
            result.Action = CommandAction.Help;
            return result;
        }

        foreach (var arg in args)
        {
            if (arg is "-h" or "--help" or "help")
            {
                result.Action = CommandAction.Help;
                return result;
            }

            if (arg.Equals("configure", StringComparison.OrdinalIgnoreCase)) result.Action = CommandAction.Configure;
            else if (arg.Equals("build", StringComparison.OrdinalIgnoreCase)) result.Action = CommandAction.Build;
            else if (arg.Equals("export", StringComparison.OrdinalIgnoreCase)) result.Action = CommandAction.Export;
            else if (arg.Equals("publish", StringComparison.OrdinalIgnoreCase)) result.Action = CommandAction.Publish;
            else if (arg.Equals("clean", StringComparison.OrdinalIgnoreCase)) result.Action = CommandAction.Clean;
            else if (arg.StartsWith("--target=", StringComparison.OrdinalIgnoreCase))
            {
                var platformStr = arg.Substring(9).ToLower();
                result.Target = platformStr switch
                {
                    "win64" or "windows" => TargetPlatform.Win64,
                    "linux" => TargetPlatform.Linux,
                    "mac" or "macos" or "osx" => TargetPlatform.Mac,
                    _ => result.Target
                };
            }
            else if (arg.StartsWith("--config=", StringComparison.OrdinalIgnoreCase))
            {
                var configStr = arg.Substring(9).ToLower();
                result.Config = configStr switch
                {
                    "debug" => BuildConfiguration.Debug,
                    "release" => BuildConfiguration.Release,
                    "development" or "dev" => BuildConfiguration.Development,
                    "shipping" => BuildConfiguration.Shipping,
                    _ => result.Config
                };
            }
        }

        return result;
    }

    public static void PrintHelp()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
===================================================================
                   BASALT ENGINE BUILD SYSTEM                     
===================================================================");
        Console.ResetColor();
        Console.WriteLine(@"
USAGE:
  BasaltBuild <command> [options]

COMMANDS:
  configure    Generates native build scripts (Visual Studio / Ninja)
  build        Configures and compiles all engine & game modules
  export       Builds and packages a standalone game release for players
  publish      Compiles and packages the Basalt Engine SDK for developers
  clean        Deletes all generated build files, solutions, and binaries
  new <Name>   Generates a new game project folder structure
  -h, --help   Displays this help menu

OPTIONS:
  --target=<win64|linux|mac>     Sets the compilation target platform (Default: Host OS)
  --config=<debug|release|dev|shipping>
                                 Sets build configuration (Default: Debug)

EXAMPLES:
  BasaltBuild clean
  BasaltBuild configure --target=win64
  BasaltBuild build --config=release
");
    }
}
