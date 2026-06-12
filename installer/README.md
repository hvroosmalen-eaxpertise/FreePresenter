Prerequisites
- .NET 10 SDK
- WiX Toolset v3 (heat.exe, candle.exe, light.exe) on PATH
- PowerShell (pwsh.exe)

Build
1. From repository root:
   pwsh.exe .\installer\build.ps1

Output
- installer\FreePresenter.msi

Test
- Double-click the MSI to install.
- Confirm Start Menu -> FreePresenter shortcut launches the app.
- Verify Add/Remove Programs lists FreePresenter.
- Uninstall via Settings -> Apps or Control Panel.
