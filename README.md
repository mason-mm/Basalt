# Basalt Engine

Basalt is a cross-platform C++ game engine designed for performance, modularity, and rapid iteration. It features **BasaltBuild Tool (BBT)**—a custom C# build system that orchestrates cross-platform project generation, workspace configuration, and native compilation across Windows, Linux, and macOS.

---

## Key Features

* **Modular Engine Architecture**: Clean component separation using YAML-based module declarations (`Build.basalt`)[cite: 10].
* **Dual Build Backends**: Seamlessly generate and compile projects using either **MSBuild** (Visual Studio solutions) or **Ninja** (fast parallel builds)[cite: 4, 5].
* **Clean Workspace Isolation**: Automatically routes intermediate build files (`Intermediate/`) and compiled binaries (`Binaries/`) away from the project root directory[cite: 1, 4, 5, 7].

---

## Documentation Links

* [System Requirements](Requirements.md)
* [Building From Source](BuildFromSource.md)

---

## Quick CLI Usage

```bash
# Display help and command options[cite: 2]
BasaltBuild -h

# Configure workspace build files[cite: 2]
BasaltBuild configure --backend=msbuild --platform=Win64 --config=Debug

# Compile binaries using the selected backend
BasaltBuild build --backend=ninja --config=Debug

# Clean all generated build artifacts and intermediate caches
BasaltBuild clean
```