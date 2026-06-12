Param(
	[string]$PublishDir = "..\..\publish\win-x64",
	[string]$PackageOutput = "..\..\installer\FreePresenter.msix",
	[string]$PackageName = "FreePresenter",
	[string]$Publisher = "CN=FreePresenter",
	[string]$Version = "1.0.0.0",
	[string]$AppId = "FreePresenter.App"
)

# Requires MakeAppx.exe and SignTool (part of Windows SDK)

$manifest = @"
<Package
  xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
  xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
  IgnorableNamespaces="uap">
  <Identity Name="$PackageName" Publisher="$Publisher" Version="$Version" />
  <Properties>
	<DisplayName>$PackageName</DisplayName>
	<PublisherDisplayName>FreePresenter</PublisherDisplayName>
	<Description>FreePresenter presentation app</Description>
  </Properties>
  <Dependencies>
	<TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.16299.0" MaxVersionTested="10.0.99999.0" />
  </Dependencies>
  <Applications>
	<Application Id="$AppId" Executable="FreePresenter.exe" EntryPoint="Windows.FullTrustApplication">
	  <uap:VisualElements DisplayName="$PackageName" Description="FreePresenter" Square150x150Logo="Assets\StoreLogo.png" BackgroundColor="#FFFFFF">
	  </uap:VisualElements>
	</Application>
  </Applications>
</Package>
"@

$manifestPath = Join-Path $PublishDir "AppxManifest.xml"
Set-Content -Path $manifestPath -Value $manifest -Encoding UTF8

# Create the package
# MakeAppx.exe must be available in PATH (from Windows SDK)
$makeAppx = "makeappx.exe"
& $makeAppx pack /d $PublishDir /p $PackageOutput

# Sign the package - requires SignTool and a PFX cert
# signtool sign /fd SHA256 /a /f <path-to-pfx> $PackageOutput

Write-Host "MSIX package created at: $PackageOutput"
