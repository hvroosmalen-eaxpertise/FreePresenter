param()

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
	Write-Error "This installer requires administrative privileges. Run PowerShell as Administrator."
	exit 1
}

$installDir = "C:\Program Files\FreePresenter"
Write-Host "Installing to $installDir"
if (-not (Test-Path $installDir)) { New-Item -ItemType Directory -Path $installDir -Force | Out-Null }

Copy-Item -Path (Join-Path $PSScriptRoot "artifacts\publish\*") -Destination $installDir -Recurse -Force

# Create Start Menu shortcut (All Users)
$WshShell = New-Object -ComObject WScript.Shell
$ProgramsFolder = [Environment]::GetFolderPath('CommonPrograms')
$lnkDir = Join-Path $ProgramsFolder 'FreePresenter'
if (-not (Test-Path $lnkDir)) { New-Item -ItemType Directory -Path $lnkDir -Force | Out-Null }
$lnkPath = Join-Path $lnkDir 'FreePresenter.lnk'
$shortcut = $WshShell.CreateShortcut($lnkPath)
$shortcut.TargetPath = Join-Path $installDir 'FreePresenter.exe'
$shortcut.WorkingDirectory = $installDir
$shortcut.Save()

# Register simple uninstall entry
$uninstallCmd = "powershell -ExecutionPolicy Bypass -File `"$installDir\\uninstall.ps1`""
New-Item -Path "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\FreePresenter" -Force | Out-Null
Set-ItemProperty -Path "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\FreePresenter" -Name "DisplayName" -Value "FreePresenter"
Set-ItemProperty -Path "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\FreePresenter" -Name "UninstallString" -Value $uninstallCmd

Write-Host "Installation complete. Use Control Panel or Settings -> Apps to uninstall, or run uninstall.ps1."
