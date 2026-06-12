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

Contributing

- Fork the repository and open a pull request for changes.
- Follow the existing code style and include tests for new functionality.
- Update this README when adding new instructions, scripts, or CI integration.

License

- Add a `LICENSE` file and update this section with the chosen license (for example: MIT).

Support / Contact

- Open issues on the repository for bug reports or feature requests.
