; Inno Setup 6 — RhosTool installer
; Compile: ISCC.exe installer\RhosTool.iss
; Ou via: .\installer\publish-installer.ps1

#define MyAppName "RhosTool"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "RhosTool"
#define MyAppExeName "RhosTool.exe"
#define MyAppURL "https://github.com/rhos-42/RhosTool"

[Setup]
AppId={{A8E3C1F2-9B4D-4E7A-8C2F-1D5E6A9B0C3D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
DefaultDirName={localappdata}\Programs\RhosTool
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=RhosTool-Setup
SetupIconFile=
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoProductName={#MyAppName}

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Créer un raccourci sur le Bureau"; GroupDescription: "Raccourcis:"; Flags: checkedonce

[Files]
Source: "..\dist\publish-single\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\dist\publish-single\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "{#MyAppExeName}"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Désinstaller {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Lancer RhosTool"; Flags: nowait postinstall skipifsilent
