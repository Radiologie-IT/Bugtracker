# Building Bugtracker

## Requirements

| Requirement | Version |
|---|---|
| OS | Windows 10 / Windows 11 |
| .NET SDK | 10.0 |
| Platform | **x64 only** (required by ScreenRecorderLib) |

Download the .NET 10 SDK from https://dotnet.microsoft.com/download/dotnet/10.0

Verify your installation:
```bat
dotnet --version
```

---

## Optional dependencies

### BugTrackerUploader (enables web upload target)

Web upload support is **fully optional**. The project detects the library at build
time and enables or disables the feature automatically — no changes to any source
file are needed.

To enable it, place the following DLLs in the `lib\` folder inside this directory:

```
lib\BugTrackerUploader.dll
lib\Newtonsoft.Json.dll
```

When both files are present, MSBuild defines the `WEBUPLOAD` compile constant,
includes the assembly references, and copies the DLLs to the output directory.
The `webupload` target type is then available in the XML configuration.

When the `lib\` folder is absent or empty, the build proceeds without any
modification and the `webupload` target type is simply unavailable. Any
`type="webupload"` entries in the config are skipped with a logged warning at
runtime.

The DLLs are produced by the **BugTrackerUploader** repository (`build-all.bat`
outputs them to `publish\lib\`).

### Bugtracker Diagnostics UI plugin (optional GUI)

The application loads plugins at runtime from the `plugins\` folder.
Without the GUI plugin, the application runs in console-only mode.

To include the GUI plugin, build `BugtrackerDiagnosticsUI.dll` from the
**Bugtracker Diagnostics UI** repository and copy it to `plugins\` before
running the final build step (so the project copies it to the output directory).

```
plugins\BugtrackerDiagnosticsUI.dll
```

See the **Bugtracker Diagnostics UI** repository for build instructions.

---

## Build (console-only, no GUI plugin)

All commands are run from the `Bugtracker\` directory.

### 1. Restore NuGet packages

```bat
dotnet restore Bugtracker.csproj
```

### 2. Build — Debug

```bat
dotnet build Bugtracker.csproj -p:Platform=x64 -p:Configuration=Debug
```

Output: `bin\x64\Debug\net10.0-windows10.0.22621.0\BugtrackerSystem.exe`

### 3. Build — Release

```bat
dotnet build Bugtracker.csproj -p:Platform=x64 -p:Configuration=Release
```

Output: `bin\x64\Release\net10.0-windows10.0.22621.0\BugtrackerSystem.exe`

---

## Build (with GUI plugin)

When the GUI plugin DLL is available, the full build sequence is:

### Step 1 — Initial build (generates types needed by the plugin)

```bat
dotnet build Bugtracker.csproj -p:Platform=x64 -p:Configuration=Debug
```

### Step 2 — Build the UI plugin

Build the **Bugtracker Diagnostics UI** project. See that repository's `BUILD.md`.

### Step 3 — Copy the plugin DLL

```bat
copy /Y "..\BugtrackerDiagnosticsUI\bin\x64\Debug\net10.0-windows10.0.22621.0\BugtrackerDiagnosticsUI.dll" "plugins\"
```

For a Release build:
```bat
copy /Y "..\BugtrackerDiagnosticsUI\bin\x64\Release\net10.0-windows10.0.22621.0\BugtrackerDiagnosticsUI.dll" "plugins\"
```

### Step 4 — Final build (embeds the plugin into the output)

```bat
dotnet build Bugtracker.csproj -p:Platform=x64 -p:Configuration=Debug
```

The plugin DLL is now present in the output directory alongside the executable.

---

## Running the application

### GUI mode (default)

```bat
bin\x64\Debug\net10.0-windows10.0.22621.0\BugtrackerSystem.exe
```

### Console-only mode (skip plugin loading)

```bat
bin\x64\Debug\net10.0-windows10.0.22621.0\BugtrackerSystem.exe -sp
```

### Single command (capture all and exit)

```bat
bin\x64\Debug\net10.0-windows10.0.22621.0\BugtrackerSystem.exe capture -full
```

---

## Clean

```bat
dotnet clean Bugtracker.csproj -p:Platform=x64
```

---

## NuGet packages used

| Package | Version |
|---|---|
| `CommunityToolkit.WinUI.Notifications` | 7.1.2 |
| `Microsoft.PowerShell.SDK` | 7.5.4 |
| `Microsoft.Windows.Compatibility` | 10.0.0 |
| `MouseKeyHook` | 5.7.1 |
| `ScreenRecorderLib` | 6.6.0 |
