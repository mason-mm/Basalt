// Copyright (c) 2026 Mason Maday. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for full license information.

using System;
using BasaltBuild.Util;
using BasaltBuild.CLI;

/// <summary>
/// Handles command line inputs and parameters and forwards it the CommandLineHelper.
/// </summary>

namespace BasaltBuild.CLI;

/// <summary>
/// Handles parsing arguments.
/// </summary>
public static class CommandLineParser
{
    /// <summary>
    /// Runs the parser.
    /// </summary>
    /// <param name="args">The args passed in Main.</param>
    public static void Run(string[] args)
    {
        if (args.Length == 0)
        {
            CommandLineHelper.PrintHelp();
            return;
        }

        string command = args[0].ToLowerInvariant();
        string rootDir = Directory.GetCurrentDirectory();

        // Parse optional arguments.
        string backend = GetFlag(args, "--backend", "msbuild");
        TargetPlatform target = Enum.TryParse<TargetPlatform>(GetFlag(args, "--platform", "Win64"), true, out var parsedPlatform) ? parsedPlatform : TargetPlatform.Win64;
        BuildConfiguration config = Enum.TryParse<BuildConfiguration>(GetFlag(args, "--config", "Debug"), true, out var parsedConfig) ? parsedConfig : BuildConfiguration.Debug;

        // Run the correct command.
        switch (command)
        {
            case "configure":
                CommandLineHelper.Configure(rootDir, backend, target);
                break;
            case "build":
                CommandLineHelper.Build(rootDir, backend, config);
                break;
            case "clean":
                CommandLineHelper.Clean(rootDir);
                break;
            case "publish":
                CommandLineHelper.Publish(rootDir, target, config);
                break;
            case "export":
                CommandLineHelper.Export(rootDir, target, config);
                break;
            case "-h":
                CommandLineHelper.PrintHelp();
                break;
            case "--help":
                CommandLineHelper.PrintHelp();
                break;
            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Error] Unknown command: '{command}'");
                Console.ResetColor();
                CommandLineHelper.PrintHelp();
                break;
        }
    }

    /// <summary>
    /// Returns the flag based on given inputs.
    /// </summary>
    /// <param name="args">The arguments from Main.</param>
    /// <param name="flagName">The flag name to look for.</param>
    /// <param name="defaultValue">The default value if the flag is not there.</param>
    /// <returns>The flag.</returns>
    private static string GetFlag(string[] args, string flagName, string defaultValue)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith($"{flagName}=", StringComparison.OrdinalIgnoreCase))
            {
                return args[i].Substring(flagName.Length + 1);
            }
            if (args[i].Equals(flagName, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }
        return defaultValue;
    }
}