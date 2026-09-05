#ifndef AppVersion
  #error AppVersion must be passed by tools/release.ps1
#endif
#ifndef PublishDir
  #define PublishDir "..\dist\publish"
#endif
#ifndef OutputPath
  #define OutputPath "..\dist"
#endif
[Setup]
AppId={{405FB74C-DDEE-4B24-9C92-6AE3E367D663}
AppName=PixelRick
AppVersion={#AppVersion}
AppPublisher=PixelRick contributors
DefaultDirName={localappdata}\Programs\PixelRick
DefaultGroupName=PixelRick
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir={#OutputPath}
OutputBaseFilename=PixelRick-Setup
SetupIconFile=..\Assets\app.ico
UninstallDisplayIcon={app}\PixelRick.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
VersionInfoVersion={#AppVersion}
LicenseFile=..\LICENSE
InfoBeforeFile=..\ARTWORK-NOTICE.md
MinVersion=10.0
[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Shortcuts:"; Flags: unchecked
[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ARTWORK-NOTICE.md"; DestDir: "{app}"; Flags: ignoreversion
[Icons]
Name: "{group}\PixelRick"; Filename: "{app}\PixelRick.exe"
Name: "{autodesktop}\PixelRick"; Filename: "{app}\PixelRick.exe"; Tasks: desktopicon
[Run]
Filename: "{app}\PixelRick.exe"; Description: "Launch PixelRick"; Flags: nowait postinstall skipifsilent
[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "PixelRick"; Flags: uninsdeletevalue
; AppData configuration is deliberately preserved on uninstall.
