// Copyright (c) 2026 Mason Maday. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using BasaltBuild.Util;

/// <summary>
/// Base class for pipelines with the needed methods.
/// </summary>
namespace BasaltBuild.Pipelines;

public abstract class PipelineBase
{
    /// <summary>
    /// Master method to run the pipeline.
    /// </summary>
    /// <param name="context">The pipeline execution context.</param>
    public abstract void Execute(PipelineContext context);

    /// <summary>
    /// Copies a directory.
    /// </summary>
    /// <param name="sourceDir">The directory to copy.</param>
    /// <param name="destinationDir">The directory to copy to.</param>
    public static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir); // Make sure the destination exists.
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
        }
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            CopyDirectory(subDir, Path.Combine(destinationDir, Path.GetFileName(subDir)));
        }
    }
}