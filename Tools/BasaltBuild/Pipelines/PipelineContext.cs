// Copyright (c) 2026 Mason Maday. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for full license information.

using System.Collections.Generic;
using BasaltBuild.Util;

namespace BasaltBuild.Pipelines;

/// <summary>
/// Encapsulates the execution context for pipelines.
/// </summary>
public class PipelineContext
{
    public required string RootDir { get; set; }
    public required List<(string ModulePath, ModuleData Data)> Modules { get; set; }
    public TargetPlatform Target { get; set; }
    public BuildConfiguration Config { get; set; }
}
