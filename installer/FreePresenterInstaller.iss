; Inno Setup script for FreePresenter
; Produces a single installer that installs the appropriate self-contained publish for x86 or x64

; Compiler warnings observed during a previous compile:
; Warning: Architecture identifier "x64" is deprecated. Substituting "x64os", but note that "x64compatible" is preferred in most cases.
;   Suggestion: use ArchitecturesInstallIn64BitMode=x64os or x64compatible to avoid the deprecation warning.
; Warning: Constant "pf" has been renamed. Use "commonpf" instead or consider using its "auto" form.
;   Suggestion: replace DefaultDirName={pf}\FreePresenter with DefaultDirName={commonpf}\FreePresenter or {autopf}.

#define ProjectRoot "{src}\.."

[Setup]
AppName=FreePresenter
AppVersion=1.0.0
DefaultDirName={pf}\FreePresenter
DefaultGroupName=FreePresenter
OutputBaseFilename=FreePresenter_Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64 x86
ArchitecturesInstallIn64BitMode=x64

[Files]
; x64 publish files (installed only on 64-bit OS)
; Source paths are relative to the .iss script location, so use ..\publish\...
Source: "..\\publish\\win-x64\\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion; Check: IsWin64
; x86 publish files (installed only on 32-bit OS)
Source: "..\\publish\\win-x86\\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion; Check: not IsWin64
; also include the icon explicitly (if not already in the publish output)
Source: "..\\assets\\FreePresenter.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\\FreePresenter"; Filename: "{app}\\FreePresenter.exe"; WorkingDir: "{app}"; IconFilename: "{app}\\FreePresenter.ico"
Name: "{commondesktop}\\FreePresenter"; Filename: "{app}\\FreePresenter.exe"; Tasks: desktopicon; IconFilename: "{app}\\FreePresenter.ico"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked

[Code]
function IsWin64(): Boolean;
begin
  Result := IsWin64;
end;
