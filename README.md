# Basalt Engine

Basalt is a cross-platform C++ game engine designed for performance, modularity, and rapid iteration. It features **BasaltBuild Tool (BBT)**—a custom Native AOT C# build system that orchestrates cross-platform project generation, workspace configuration, and native compilation across Windows, Linux, and macOS.

---

## BasaltBuild Tool (BBT)

BBT manages workspace configuration, cross-platform build outputs, and project creation without relying on heavy external build toolchains.

### CLI Usage

```bash
# Display help and command options
BasaltBuild -h

# Create a new game project
BasaltBuild new <ProjectName> [TargetDirectory]

# Configure workspace build files (Visual Studio .sln on Win64 / build.ninja on Linux & macOS)
BasaltBuild configure [--target=win64|linux|mac] [--config=debug|release|dev|shipping]

# Configure and compile all engine and game modules
BasaltBuild build [--target=win64|linux|mac] [--config=debug|release|dev|shipping]

# Package a standalone game release
BasaltBuild export

# Package the Basalt Engine SDK
BasaltBuild publish

# Clean all generated build artifacts, intermediate caches, and solution files
BasaltBuild clean
```

---

## Getting Started

### Prerequisites

* **Windows**: Visual Studio 2022 (with Desktop Development with C++) or MSVC / Clang.
* **Linux / macOS**: `clang++` or `g++`, and `ninja`.
* **.NET SDK**: .NET 10 (or latest SDK) to build or modify the `BasaltBuild` CLI tool.

### Building BBT from Source

To compile the `BasaltBuild` CLI executable as a standalone Native AOT binary:

```powershell
# Windows
dotnet publish Tools/BasaltBuild/BasaltBuild.csproj -c Release -r win-x64 -o Tools/BasaltBuild/Binaries/Windows

# Linux
dotnet publish Tools/BasaltBuild/BasaltBuild.csproj -c Release -r linux-x64 -o Tools/BasaltBuild/Binaries/Linux

# macOS
dotnet publish Tools/BasaltBuild/BasaltBuild.csproj -c Release -r osx-x64 -o Tools/BasaltBuild/Binaries/Mac
```
---
