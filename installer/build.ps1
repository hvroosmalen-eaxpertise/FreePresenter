<#
Build script for FreePresenter MSI (WiX v3 CLI path)
Run from repository root:
pwsh.exe .\installer\build.ps1
#>

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$publishDir = Join-Path $scriptDir 'artifacts\publish'
$harvestFile = Join-Path $scriptDir 'Harvest.wxs'
$shortcutFragment = Join-Path $scriptDir 'ShortcutFragment.wxs'
$productWxs = Join-Path $scriptDir 'FreePresenter.Setup\Product.wxs'
$outMsi = Join-Path $scriptDir 'FreePresenter.msi'
$wixObjDir = Join-Path $scriptDir 'obj'

# Ensure WiX CLI tools exist
function Ensure-Tool($name) {
	return (Get-Command $name -ErrorAction SilentlyContinue) -ne $null
}

$hasHeat = Ensure-Tool heat.exe
$hasCandle = Ensure-Tool candle.exe
$hasLight = Ensure-Tool light.exe

Write-Host "WiX tools present: heat=$hasHeat, candle=$hasCandle, light=$hasLight"

# Locate project file automatically
$repoRoot = Split-Path -Parent $scriptDir
$csproj = Get-ChildItem -Path $repoRoot -Recurse -Filter *.csproj -ErrorAction SilentlyContinue |
	Where-Object { $_.FullName -notmatch '\\installer\\' } |
	Select-Object -First 1
if ($null -eq $csproj) {
	Write-Error "No .csproj found under repository root ($repoRoot). Cannot publish."
}

$projectPath = $csproj.FullName
Write-Host "Publishing self-contained win-x64 from project: $projectPath"
dotnet publish $projectPath -c Release -r win-x64 --self-contained $true -o $publishDir

if (-not (Test-Path $publishDir)) {
	Write-Error "Publish output not found at $publishDir"
}

# Clean previous generated files
Remove-Item -Force -ErrorAction SilentlyContinue $harvestFile, $shortcutFragment, $outMsi
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $wixObjDir

if ($hasHeat -and $hasCandle -and $hasLight) {
	# Harvest publish folder with heat
	Write-Host "Harvesting files with heat..."
	& heat.exe dir $publishDir -cg AppFiles -dr INSTALLFOLDER -srd -ag -gg -sfrag -var var.SourceDir -out $harvestFile

	if (-not (Test-Path $harvestFile)) {
		Write-Error "Heat failed to produce $harvestFile"
	}

	# Find the File Id for the main exe in the harvested file
	$exeName = 'FreePresenter.exe'
	$harvestDoc = [xml](Get-Content $harvestFile -Raw)
	$ns = @{ wix = 'http://schemas.microsoft.com/wix/2006/wi' }
	$exeFile = Select-Xml -Xml $harvestDoc -XPath "//wix:File[contains(@Source, '$exeName')]" -Namespace $ns
	if ($exeFile) {
		$exeFileId = $exeFile.Node.Id
		Write-Host "Found EXE file id: $exeFileId"
	} else {
		Write-Error "Could not find $exeName in $harvestFile. Ensure the published exe name matches."
	}

	# Generate shortcut fragment referencing harvested File Id
	$shortcutContent = @"
<?xml version="1.0" encoding="utf-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Fragment>
	<DirectoryRef Id="ApplicationProgramsFolder">
	  <Component Id="cmpStartMenuShortcut" Guid="*">
		<FileRef Id="$exeFileId">
		  <Shortcut Id="StartMenuShortcut"
					Name="FreePresenter"
					Description="FreePresenter"
					Directory="ApplicationProgramsFolder"
					WorkingDirectory="INSTALLFOLDER"
					Advertise="no" />
		</FileRef>
		<RemoveFolder Id="RemoveProgramMenuDir" Directory="ApplicationProgramsFolder" On="uninstall"/>
		<RegistryValue Root="HKCU" Key="Software\FreePresenter" Name="installed" Type="integer" Value="1" KeyPath="yes"/>
	  </Component>
	</DirectoryRef>

	<ComponentGroup Id="Shortcuts">
	  <ComponentRef Id="cmpStartMenuShortcut"/>
	</ComponentGroup>
  </Fragment>
</Wix>
"@

	Set-Content -Path $shortcutFragment -Value $shortcutContent -Encoding UTF8
	Write-Host "Generated shortcut fragment at $shortcutFragment"

	# Compile (.wixobj) and link (MSI)
	New-Item -ItemType Directory -Force -Path $wixObjDir | Out-Null

	Write-Host "Running candle..."
	& candle.exe -dSourceDir="$publishDir" -out (Join-Path $wixObjDir '') $productWxs $harvestFile $shortcutFragment

	Write-Host "Running light..."
	& light.exe -out $outMsi (Join-Path $wixObjDir '*.wixobj')

	if (Test-Path $outMsi) {
		Write-Host "MSI created: $outMsi"
		exit 0
	} else {
		Write-Error "MSI build failed."
	}
} else {
	Write-Warning "WiX CLI tools not available; falling back to simple PowerShell installer and ZIP artifact."

	# Create zip of publish folder
	$zipPath = Join-Path $scriptDir 'FreePresenter.zip'
	if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
	Write-Host "Creating ZIP artifact..."
	Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zipPath -Force
	Write-Host "ZIP created: $zipPath"

	# Copy fallback installer scripts into installer directory
	$installScript = Join-Path $scriptDir 'install.ps1'
	$uninstallScript = Join-Path $scriptDir 'uninstall.ps1'

	$installContent = @'
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
'@

	$uninstallContent = @'
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
'@

	Set-Content -Path $installScript -Value $installContent -Encoding UTF8
	Set-Content -Path $uninstallScript -Value $uninstallContent -Encoding UTF8

	Write-Host "Generated simple installer scripts: $installScript and $uninstallScript"
	Write-Host "To install on a machine without WiX: run PowerShell as administrator and execute: `pwsh.exe .\installer\install.ps1`"
	Write-Host "To uninstall: run PowerShell as administrator and execute: `pwsh.exe .\installer\uninstall.ps1`"
	exit 0
}
