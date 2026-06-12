# FreePresenter

FreePresenter is a simple, lightweight presentation app built on .NET 10. It lets you drag-and-drop an image or video file onto the main window, then view it full-screen on any monitor.

## Prerequisites

- .NET 10 SDK — https://dotnet.microsoft.com
- Visual Studio 2026 with the .NET 10 workload (Windows)
- PowerShell Core (pwsh) recommended for automation

## Quickstart

```
dotnet restore FreePresenter.csproj
dotnet build FreePresenter.csproj -c Release
dotnet run --project FreePresenter.csproj
```

## Controls

| Key | Action |
|-----|--------|
| **F11** | Toggle viewer between full-screen and windowed mode |
| **ESC** | Exit full-screen back to windowed mode |
| **Maximize** (in windowed) | Switch to borderless full-screen |
| **Drag** title bar (main window) | Move the window |

- Drop an image or video file onto the main window to open it.
- The viewer opens in windowed mode (1280×720) by default.
- Close the viewer (✕ or ESC in windowed) to return to the main window.

## Installer

Build scripts are in `installer/`:

- **WiX MSI** — run `pwsh.exe .\installer\build.ps1` to produce `FreePresenter.msi`
- **Fallback** — the same script creates a ZIP + install/uninstall PowerShell scripts if WiX is not available
- **MSIX** — see `installer/msix/` for packaging via MakeAppx

## CI

| Workflow | Status |
|----------|--------|
| CI (build + publish) | [![CI](https://github.com/hvroosmalen-eaxpertise/FreePresenter/actions/workflows/ci.yml/badge.svg)](https://github.com/hvroosmalen-eaxpertise/FreePresenter/actions/workflows/ci.yml) |
| Build MSI | [![Build MSI](https://github.com/hvroosmalen-eaxpertise/FreePresenter/actions/workflows/ci-msi.yml/badge.svg)](https://github.com/hvroosmalen-eaxpertise/FreePresenter/actions/workflows/ci-msi.yml) |

## Contributing

Fork the repo and open a pull request. Follow the existing code style.

## License

Mozilla Public License 2.0 — see [LICENSE](LICENSE).
