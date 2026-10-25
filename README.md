# Basalt Engine

Basalt is a cross-platform C++ game engine designed for easy features and fast development.

---

## Key Features

* Modular structure made possible by **BasaltBuildTool**.
* Dual compilation backend with MSBuild and Ninja.
* Easy to install/build from source.

---

## Tools
The Basalt Engine uses custom made tools build with different languages including Python3 and C#.
Basalt currently has two tools:

* BasaltBuild - The main build tools that builds the engine and games written in C#.
* BasaltDotnetHelper - A Python helper that lets users build C# tools with one single command.

---
## How to use the tools
Make sure to run these commands from the root Basalt/ directory.

### BasaltBuild
```bash
# Windows
Tools/BasaltBuild/Binaries/Win64/<YourConfig>/BasaltBuild.exe <clean|configure|build|export|publish> --config <Debug|Release|Development|Shipping> --platform Win64 --backend MSBuild

# Linux
Tools/BasaltBuild/Binaries/Linux/<YourConfig>/BasaltBuild <clean|configure|build|export|publish> --config <Debug|Release|Development|Shipping> --platform Linux --backend Ninja

# MacOS
Tools/BasaltBuild/Binaries/Mac/<YourConfig>/BasaltBuild <clean|configure|build|export|publish> --config <Debug|Release|Development|Shipping> --platform Mac --backend Ninja
```

### BasaltDotnetHelper
```bash
# Windows
python3 Tools/BasaltDotnetHelper/BasaltDotnetHelper.py <path-to-csproj> --config <Debug|Release> --platform Win64

# Linux
python3 Tools/BasaltDotnetHelper/BasaltDotnetHelper.py <path-to-csproj> --config <Debug|Release> --platform Linux

# MacOS
python3 Tools/BasaltDotnetHelper/BasaltDotnetHelper.py <path-to-csproj> --config <Debug|Release> --platform Mac
```
---
## Building from source
Here is a guide on how to easily build the engine from source.
Make sure you are running these commands from the root Basalt/ directory.
```bash
# Build BasaltBuildTool
# Windows
python3 Tools/BasaltDotnetHelper/BasaltDotnetHelper.py Tools/BasaltBuild/BasaltBuild.csproj --config Release --platform Win64

# Linux
python3 Tools/BasaltDotnetHelper/BasaltDotnetHelper.py Tools/BasaltBuild/BasaltBuild.csproj --config Release --platform Linux

# MacOS
python3 Tools/BasaltDotnetHelper/BasaltDotnetHelper.py Tools/BasaltBuild/BasaltBuild.csproj --config Release --platform Mac

# Configure and build the engine
# Windows
Tools/BasaltBuild/Binaries/Win64/Release/BasaltBuild.exe configure --config Release --platform Win64 --backend MSBuild
Tools/BasaltBuild/Binaries/Win64/Release/BasaltBuild.exe build --config Release --platform Win64 --backend MSBuild

# Linux
Tools/BasaltBuild/Binaries/Linux/Release/BasaltBuild configure --config Release --platform Linux --backend Ninja
Tools/BasaltBuild/Binaries/Linux/Release/BasaltBuild build --config Release --platform Linux --backend Ninja

# MacOS
Tools/BasaltBuild/Binaries/Mac/Release/BasaltBuild configure --config Release --platform Mac --backend Ninja
Tools/BasaltBuild/Binaries/Mac/Release/BasaltBuild build --config Release --platform Mac --backend Ninja

# Run the editor
# Windows
Binaries/Win64/Release/BasaltEditor.exe

# Linux
Binaries/Linux/Release/BasaltEditor

# MacOS
Binaries/Mac/Release/BasaltEditor
```
