; Inno Setup 6 script for MP1 Configuration Studio.
; Build with:
;   ISCC.exe /DAppVersion=1.0.0 packaging\MP1ConfigurationStudio.iss

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#define AppName "MP1 Configuration Studio"
#define AppPublisher "MP1 Configuration Studio"
#define AppExe "MP1ConfigurationStudio.exe"
#define ProjectRoot ".."

[Setup]
AppId={{8A2F6C1E-4B7D-4E2A-9C3F-7D1B5E8A9C24}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={code:GetMediaPortalDir}
DisableDirPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
OutputDir={#ProjectRoot}\dist\package
OutputBaseFilename=MP1-Configuration-Studio-{#AppVersion}-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
Uninstallable=yes
UninstallDisplayName={#AppName}
LicenseFile={#ProjectRoot}\LICENSE

[Files]
Source: "{#ProjectRoot}\dist\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\dist\{#AppExe}.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\SECURITY.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\SUPPORT.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\PRIVACY.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\docs\RELEASE-1.0.0.md"; DestDir: "{app}"; DestName: "RELEASE-NOTES.md"; Flags: ignoreversion
Source: "{#ProjectRoot}\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{group}\{#AppName} – Klassisch"; Filename: "{app}\{#AppExe}"; Parameters: "--classic"

[Run]
Filename: "{app}\{#AppExe}"; Description: "{#AppName} starten"; Flags: postinstall nowait skipifsilent

[UninstallDelete]
; Only Studio-owned files. Native MediaPortal files are never touched.
Type: files; Name: "{app}\{#AppExe}"
Type: files; Name: "{app}\{#AppExe}.config"
Type: files; Name: "{app}\README.md"
Type: files; Name: "{app}\SECURITY.md"
Type: files; Name: "{app}\SUPPORT.md"
Type: files; Name: "{app}\PRIVACY.md"
Type: files; Name: "{app}\RELEASE-NOTES.md"
Type: files; Name: "{app}\LICENSE"

[Code]
function GetMediaPortalDir(Param: String): String;
var
  Value: String;
begin
  if RegQueryStringValue(HKLM64, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal', 'InstallPath', Value) then
    Result := Value
  else if RegQueryStringValue(HKLM32, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal', 'InstallPath', Value) then
    Result := Value
  else if DirExists(ExpandConstant('{pf}\Team MediaPortal\MediaPortal')) then
    Result := ExpandConstant('{pf}\Team MediaPortal\MediaPortal')
  else if DirExists(ExpandConstant('{pf32}\Team MediaPortal\MediaPortal')) then
    Result := ExpandConstant('{pf32}\Team MediaPortal\MediaPortal')
  else
    Result := '';
end;

function InitializeSetup(): Boolean;
var
  Dir: String;
begin
  Dir := GetMediaPortalDir('');
  Result := (Dir <> '') and FileExists(Dir + '\Configuration.exe');
  if not Result then
    MsgBox('MediaPortal 1.39 x64 wurde nicht gefunden. Bitte zuerst MediaPortal installieren.', mbError, MB_OK);
end;
