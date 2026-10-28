# Copyright (c) 2026 Mason Maday. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for full license information.

import os
import platform
import subprocess
import sys

def main():
    if len(sys.argv) < 2:
        print("[BasaltDotnetHelper] Error: No project path specified.", file=sys.stderr)
        print("Usage: python BasaltDotnetHelper.py <path-to-csproj> [arguments for the tool]")
        sys.exit(1)

    csproj_path = sys.argv[1]
    if not os.path.exists(csproj_path):
        print(f"[BasaltDotnetHelper] Error: Project file not found at '{csproj_path}'.", file=sys.stderr)
        sys.exit(1)

    # Detect host operating system for target binaries layout
    system = platform.system()
    target_platform = "Win64"
    rid = "win-x64"
    if system == "Linux":
        target_platform = "Linux"
        rid = "linux-x64"
    elif system == "Darwin":
        target_platform = "Mac"
        rid = "osx-arm64"

    config = "Release"
    project_dir = os.path.dirname(os.path.abspath(csproj_path))
    bin_dir = os.path.join(project_dir, "Binaries", target_platform, config)
    exe_ext = ".exe" if system == "Windows" else ""
    project_name = os.path.splitext(os.path.basename(csproj_path))[0]
    executable_path = os.path.join(bin_dir, f"{project_name}{exe_ext}")

    print(f"[BasaltDotnetHelper] Ensuring {project_name} is up to date ({target_platform} | {config})...")

    # Automatically publish/build the C# tool on the fly
    publish_cmd = [
        "dotnet", "publish", csproj_path,
        "-c", config,
        "-r", rid,
        "-o", bin_dir,
        "/p:PublishSingleFile=true",
        "--self-contained", "true"
    ]

    result = subprocess.run(publish_cmd)
    if result.returncode != 0:
        print(f"[BasaltDotnetHelper] Error: Failed to publish {project_name}.", file=sys.stderr)
        sys.exit(result.returncode)

    # Forward all remaining command-line arguments to the built C# tool
    tool_args = sys.argv[2:]
    run_cmd = [executable_path] + tool_args

    run_result = subprocess.run(run_cmd)
    sys.exit(run_result.returncode)

if __name__ == "__main__":
    main()
