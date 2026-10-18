# Building From Source

This guide outlines how to compile and publish **BasaltBuild Tool (BBT)** and configure your workspace from scratch.

---

### Step 1: Clone the Repository

Clone your repository locally and navigate to the root directory:

```bash
git clone [https://github.com/YourUsername/Basalt.git](https://github.com/YourUsername/Basalt.git)
cd Basalt
```

---

### Step 2: Publish BasaltBuild Tool (BBT)

Compile the C# CLI tool as a standalone native binary for your target platform:

```powershell
# Windows (Win64 self-contained release)[cite: 1]
dotnet publish Tools/BasaltBuild/BasaltBuild.csproj -c Release -r win-x64
```

*(Optional)* You can add the output directory (`Tools/BasaltBuild/Binaries/Win64/Release/`) to your system environment `PATH` or create an alias for `basaltbuild`.

---

### Step 3: Configure Your Workspace

Run the `configure` command using your preferred backend (`msbuild` or `ninja`)[cite: 1, 2]:

```bash
# To generate Visual Studio MSBuild solutions[cite: 1, 4]
basaltbuild configure --backend msbuild --platform Win64 --config Debug

# OR to generate Ninja build manifests[cite: 1, 5]
basaltbuild configure --backend ninja --platform Win64 --config Debug
```

---

### Step 4: Build the Engine

Compile the engine modules and binaries[cite: 1, 2]:

```bash
basaltbuild build --backend msbuild --config Debug

# Alternatively using Ninja:
basaltbuild build --backend ninja --config Debug
```