# System Requirements

To build and run **Basalt Engine** and **BasaltBuild Tool (BBT)**, ensure your development environment meets the following specifications:

### Software Prerequisites

* **.NET SDK**: **.NET 10.0 SDK** (required to compile and publish the `BasaltBuild` CLI tool targeting `net10.0`)[cite: 1].
* **C++ Compiler Toolchain**:
  * **Windows**: Visual Studio 2022 with the *Desktop development with C++* workload installed (providing MSVC `cl.exe`, MSBuild, and the Windows 10/11 SDK)[cite: 4]. C++20 language standard conformance (`/std:c++20`) is required for engine source files[cite: 4, 10].
  * **Linux / macOS**: `clang++` or `g++` supporting C++20, along with `ninja`.
* **Build Backends**:
  * **MSBuild** (included with Visual Studio) for `.sln` and `.vcxproj` generation[cite: 4].
  * **Ninja** (optional) for high-performance parallel command-line builds[cite: 5].