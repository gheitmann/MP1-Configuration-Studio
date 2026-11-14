; Inno Setup 6 script for MP1 Configuration Studio.
; Build with:
;   ISCC.exe /DAppVersion=1.0.0 packaging\MP1ConfigurationStudio.iss

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#ifndef TestInstall
#define AppName "MP1 Configuration Studio"
#else
#define AppName "MP1 Configuration Studio Test"
#endif
#define AppPublisher "MP1 Configuration Studio"
#define AppExe "MP1ConfigurationStudio.exe"
#define ProjectRoot ".."

[Setup]
#ifdef TestInstall
AppId={{8A2F6C1E-4B7D-4E2A-9C3F-7D1B5E8A9C24-TEST}
#else
AppId={{8A2F6C1E-4B7D-4E2A-9C3F-7D1B5E8A9C24}
#endif
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
#ifdef TestInstall
PrivilegesRequired=lowest
DefaultDirName={localappdata}\MP1StudioSetupTest
#else
PrivilegesRequired=admin
DefaultDirName={code:GetMediaPortalDir}
#endif
DisableDirPage=yes
DisableProgramGroupPage=yes
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
OutputDir={#ProjectRoot}\dist\package
#ifdef TestInstall
OutputBaseFilename=MP1-Configuration-Studio-{#AppVersion}-Setup-Test
#else
OutputBaseFilename=MP1-Configuration-Studio-{#AppVersion}-Setup
#endif
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile={#ProjectRoot}\assets\MediaPortal.ico
Uninstallable=yes
UninstallDisplayName={#AppName}
UninstallDisplayIcon={app}\{#AppExe}
LicenseFile={#ProjectRoot}\LICENSE

[Components]
Name: "application"; Description: "MP1 Configuration Studio (desktop)"; Types: full compact custom; Flags: fixed
#ifndef ExcludePlugin
Name: "mpPlugin"; Description: "MediaPortal plugin (optional)"; Types: full
#endif

[Files]
Source: "{#ProjectRoot}\dist\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\dist\{#AppExe}.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\SECURITY.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\SUPPORT.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\PRIVACY.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\docs\RELEASE-{#AppVersion}.md"; DestDir: "{app}"; DestName: "RELEASE-NOTES.md"; Flags: ignoreversion
Source: "{#ProjectRoot}\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
#ifndef ExcludePlugin
Source: "{#ProjectRoot}\plugin\bin\Release\MP1.ConfigurationStudio.Plugin.dll"; DestDir: "{app}\plugins\Windows"; Flags: ignoreversion; Components: mpPlugin
Source: "{#ProjectRoot}\plugin\Skin\MP1StudioSettings.xml"; DestDir: "{app}\plugins\Windows"; Flags: ignoreversion; Components: mpPlugin
#endif

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

function GetSkinRoot(Param: String): String;
begin
#ifdef TestInstall
  Result := ExpandConstant('{localappdata}\MP1StudioSkinsTest');
#else
  Result := ExpandConstant('{commonappdata}\Team MediaPortal\MediaPortal\skin');
#endif
end;

function InitializeSetup(): Boolean;
#ifndef TestInstall
var
  Dir: String;
#endif
begin
#ifdef TestInstall
  Result := True;
#else
  Dir := GetMediaPortalDir('');
  Result := (Dir <> '') and FileExists(Dir + '\Configuration.exe');
  if not Result then
    MsgBox('MediaPortal 1.39 x64 wurde nicht gefunden. Bitte zuerst MediaPortal installieren.', mbError, MB_OK);
#endif
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  SkinRoot, PluginSkin, Script: String;
  ResultCode: Integer;
begin
  if (CurStep = ssPostInstall) and WizardIsComponentSelected('mpPlugin') then
  begin
    SkinRoot := GetSkinRoot('');
    PluginSkin := ExpandConstant('{app}\plugins\Windows\MP1StudioSettings.xml');
    if DirExists(SkinRoot) and FileExists(PluginSkin) then
    begin
      Script := 'Get-ChildItem -LiteralPath ''' + SkinRoot + ''' -Directory | ForEach-Object { Copy-Item -LiteralPath ''' + PluginSkin + ''' -Destination (Join-Path $_.FullName ''MP1StudioSettings.xml'') -Force }';
      Exec('powershell.exe', '-NoProfile -ExecutionPolicy Bypass -Command "' + Script + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    end;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  SkinRoot, Script: String;
  ResultCode: Integer;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    SkinRoot := GetSkinRoot('');
    if DirExists(SkinRoot) then
    begin
      Script := 'Get-ChildItem -LiteralPath ''' + SkinRoot + ''' -Directory | ForEach-Object { $f = Join-Path $_.FullName ''MP1StudioSettings.xml''; if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }';
      Exec('powershell.exe', '-NoProfile -ExecutionPolicy Bypass -Command "' + Script + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    end;
  end;
end;
