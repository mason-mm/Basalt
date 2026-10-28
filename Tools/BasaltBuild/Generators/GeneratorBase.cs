// Copyright (c) 2026 Mason Maday. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BasaltBuild.Util;

/// <summary>
/// Base class for generaters with required methods like ExecuteBuild and Configure.
/// </summary>

namespace BasaltBuild.Generators;

/// <summary>
/// Base class for all build generators so there is a common interface to use.
/// </summary>
public abstract class BuildGenerator
{
    /// <summary>
    /// Builds the project.
    /// </summary>
    /// <param name="rootDir">The directory to start the build form (CWD).</param>
    /// <param name="config">The config for the build like debug or release.</param>
    public abstract void ExecuteBuild(string rootDir, BuildConfiguration? config = null);

    /// <summary>
    /// Configures the project and generates build files.
    /// </summary>
    /// <param name="rootDir">The directory to start the build form (CWD).</param>
    /// <param name="modules">The list of modules to configure for.</param>
    /// <param name="target">The target platform to build for.</param>
    public abstract void Configure(string rootDir, List<(string ModulePath, ModuleData Data)> modules, TargetPlatform target);

    /// <summary>
    /// Generates a deterministic guid based on an input string like a module name.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The deterministic guid.</returns>
    protected static Guid GenerateDeterministicGuid(string input)
    {
        using var md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }

    /// <summary>
    /// Collects files using a pattern across the specified subdirectories.
    /// </summary>
    /// <param name="baseDir">The base dir to start looking from.</param>
    /// <param name="subDirs">All subdirs to look in.</param>
    /// <param name="pattern">The pattern to look for.</param>
    /// <returns>The list of files that match the requirements.</returns>
    protected static List<string> GetFiles(string baseDir, List<string> subDirs, string pattern)
    {
        var files = new List<string>();
        foreach (var subDir in subDirs)
        {
            string fullPath = Path.Combine(baseDir, subDir);
            if (Directory.Exists(fullPath))
            {
                files.AddRange(Directory.GetFiles(fullPath, pattern, SearchOption.AllDirectories));
            }
        }
        return files;
    }
}
