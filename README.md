# Basalt Engine

Basalt is a cross-platform C++ game engine designed for easy features and fast development.

---

## Key Features

* Modular structure made possible by **BasaltBuildTool**.
* Dual compilation backend with MSBuild and Ninja.
* Easy to install/build from source.

---

## How To Use BasaltBuild
```bash
# Configures the project
Tools/BasaltBuild/Binaries/YourPlatform/Release/BasaltBuild.exe configure

# Builds the project
Tools/BasaltBuild/Binaries/YourPlatform/Release/BasaltBuild.exe build

# Exports your game
Tools/BasaltBuild/Binaries/YourPlatform/Release/BasaltBuild.exe export

# Publishes the engine
Tools/BasaltBuild/Binaries/YourPlatform/Release/BasaltBuild.exe publish

# Cleans up build files
Tools/BasaltBuild/Binaries/YourPlatform/Release/BasaltBuild.exe clean
```

### Flags
```text
--backend  # Ninja | MSBuild (Default MSBuild)
--platform # Win64 | Linux | Mac (Default Win64)
--config   # Debug | Release | Development | Shipping (Default Debug)
```

### Examples
```bash
Tools/BasaltBuild/Binaries/YourPlatform/Release/BasaltBuild.exe configure --backend MSBuild --platform Win64 --config Debug

Tools/BasaltBuild/Binaries/YourPlatform/Release/BasaltBuild.exe configure --backend Ninja --platform Linux --config Shipping
```

## Building From Source
Here is an example on how to build the engine from source based on your platform

```bash
# Clone the repository
git clone https://github.com/mason-mm/Basalt.git
cd Basalt

# Build BBT

# Windows (64-bit)
dotnet publish Tools/BasaltBuild/BasaltBuild.csproj -c Release -r win-x64

# Windows (32-bit)
dotnet publish Tools/BasaltBuild/BasaltBuild.csproj -c Release -r win-x86

# Windows (ARM64)
dotnet publish Tools/BasaltBuild/BasaltBuild.csproj -c Release -r win-arm64

# Linux (64-bit)
dotnet publish Tools/BasaltBuild/BasaltBuild.csproj -c Release -r linux-x64

# Linux (ARM64)
dotnet publish Tools/BasaltBuild/BasaltBuild.csproj -c Release -r linux-arm64

# Linux (Alpine)
dotnet publish Tools/BasaltBuild/BasaltBuild.csproj -c Release -r linux-musl-x64

# MacOS (Apple Silicon / M1, M2, M3, M4)
dotnet publish Tools/BasaltBuild/BasaltBuild.csproj -c Release -r osx-arm64

# MacOS (Intel)
dotnet publish Tools/BasaltBuild/BasaltBuild.csproj -c Release -r osx-x64

# Configure the engine

# Windows
./Tools/BasaltBuild/Binaries/YourPlatform/Release/BasaltBuild.exe configure --backend MSBuild --config Release

# Linux
./Tools/BasaltBuild/Binaries/YourPlatform/Release/BasaltBuild configure --backend Ninja --config Release

# MacOS
./Tools/BasaltBuild/Binaries/YourPlatform/Release/BasaltBuild configure --backend Ninja --config Release

# Build the engine

# Windows
./Tools/BasaltBuild/Binaries/YourPlatform/Release/BasaltBuild.exe build --backend MSBuild --config Release

# Linux
./Tools/BasaltBuild/Binaries/YourPlatform/Release/BasaltBuild build --backend Ninja --config Release

# MacOS
./Tools/BasaltBuild/Binaries/YourPlatform/Release/BasaltBuild build --backend Ninja --config Release

# Run the editor

# Windows
./Binaries/YourPlatform/Release/BasaltEditor.exe

# Linux
./Binaries/YourPlatform/Release/BasaltEditor

# MacOS
./Binaries/YourPlatform/Release/BasaltEditor
```