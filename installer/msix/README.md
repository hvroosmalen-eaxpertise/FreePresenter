MSIX packaging for FreePresenter

This directory contains guidance and sample scripts to create an MSIX package for FreePresenter.

Prerequisites

- Windows 10/11 SDK with MSIX Packaging Tool or MSIX SDK
- PowerShell 7+ (pwsh)
- SignTool and a code signing certificate for production (or use test certificates for local testing)

Steps

1. Produce publish outputs for both architectures:

   pwsh> dotnet publish -c Release -r win-x64 -o ..\..\publish\win-x64
   pwsh> dotnet publish -c Release -r win-x86 -o ..\..\publish\win-x86

2. Use the MSIX Packaging Tool (GUI) to create a package by pointing it to the published folder, or use the MSIX SDK with a packaging.ps1 script.

3. Sign the package:

   signtool sign /fd SHA256 /a /f <your-pfx-file> <path-to-msix>

4. Test the package by installing it locally:

   Add-AppxPackage -Path <path-to-msix> -ForceApplicationShutdown

Notes

- MSIX requires that applications are not installed to arbitrary Program Files directories; the package manages installation.
- For CI, prefer the MSIX SDK or MSIX Packaging Tool CLI for automated packaging.
