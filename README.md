# FreePresenter

FreePresenter is a simple, lightweight presentation app built on .NET 10. This repository contains the application source, project files, installer scripts, and assets used to build and distribute the app.

Badges

- CI: (add your CI badge)
- Package: (add package badge if applicable)

Summary

FreePresenter aims to provide a minimal, extensible presentation experience using .NET 10 technologies. The repo includes source code, sample content, and packaging helpers.

Prerequisites

- .NET 10 SDK — https://dotnet.microsoft.com
- Visual Studio 2026 (Community/Professional/Enterprise) with the .NET 10 workload (Windows)
- PowerShell Core (pwsh) recommended for automation on Windows/macOS/Linux

Quickstart — CLI (dotnet)

1. Open a terminal in the repository root.
2. Restore dependencies:

```
dotnet restore
```

3. Build the solution or projects:

```
dotnet build -c Release
```

4. Run an executable project (replace the project path if needed):

```
dotnet run --project src/FreePresenter/FreePresenter.csproj
```

Testing

- Run unit tests:

```
dotnet test
```

Visual Studio (Windows) — Build & Run

1. Open `FreePresenter.sln` in Visual Studio 2026.
2. Ensure the .NET 10 workloads are installed.
3. Set the desired startup project, then use Build > Build Solution (Ctrl+Shift+B) and Debug > Start Debugging (F5) or Start Without Debugging (Ctrl+F5).

Packaging / Installer

- If this repo includes an installer script (Inno Setup), see the `installer/` directory for `.iss` files and instructions.
- Example publish command (produce artifacts for distribution):

```
dotnet publish -c Release -r win-x64 --self-contained false -o ./artifacts
```

Installer (WiX / fallback)

- This repository now contains a WiX-based installer workflow under the `installer/` folder.
- Primary build path: WiX Toolset CLI (heat/candle/light) will be used to build an MSI when available on PATH.
- Fallback: if WiX CLI is not present the build script will produce a self-contained publish ZIP and simple PowerShell installer scripts.
- To build the installer or fallback artifacts, run from the repository root in PowerShell:

```
pwsh.exe .\installer\build.ps1
```

Output (depending on environment):
- `installer/FreePresenter.msi` (if WiX CLI available)
- `installer/FreePresenter.zip`, `installer/install.ps1`, `installer/uninstall.ps1` (fallback)

See `installer/README.md` for prerequisites and details.

Controls

- Viewer window behavior:
  - F11: toggle fullscreen / windowed mode for the viewer.
  - ESC: when in fullscreen restores the viewer to windowed mode (does not close the window).
  - Maximize button: switches the viewer to borderless fullscreen on that monitor.
  - Close button: visible in windowed mode; closes the viewer when pressed.
  - The top bar on the main window is draggable to move the window.


Contributing

- Fork the repository and open a pull request for changes.
- Follow the existing code style and include tests for new functionality.
- Update this README when adding new instructions, scripts, or CI integration.

License

- Add a `LICENSE` file and update this section with the chosen license (for example: MIT).

Support / Contact

- Open issues on the repository for bug reports or feature requests.
