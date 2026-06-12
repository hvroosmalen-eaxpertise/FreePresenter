param()

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
	Write-Error "Uninstall requires administrative privileges. Run PowerShell as Administrator."
	exit 1
}

$installDir = "C:\Program Files\FreePresenter"
Write-Host "Removing $installDir"
if (Test-Path $installDir) { Remove-Item -Path $installDir -Recurse -Force }

# Remove Start Menu shortcut folder
$ProgramsFolder = [Environment]::GetFolderPath('CommonPrograms')
$lnkDir = Join-Path $ProgramsFolder 'FreePresenter'
if (Test-Path $lnkDir) { Remove-Item -Path $lnkDir -Recurse -Force }

# Remove uninstall registry key
Remove-Item -Path "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\FreePresenter" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Uninstall complete."
