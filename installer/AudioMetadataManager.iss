; Script de Inno Setup para Audio Metadata Manager.
;
; Genera un instalador real (setup.exe con asistente) a partir de la
; publicacion autocontenida de un solo archivo producida por
; `dotnet publish` (ver build-windows.ps1).
;
; No compilar este script directamente sin publicar antes: espera
; encontrar AudioMetadataManager.UI.exe en ..\publish\AudioMetadataManager.

#define MyAppName "Audio Metadata Manager"
#define MyAppVersion "0.2"
#define MyAppExeName "AudioMetadataManager.UI.exe"
#define MyPublishDir "..\publish\AudioMetadataManager"

[Setup]
AppId={{B3B6B6D4-7B7B-4B7B-9C1D-3C7C1E2A9F10}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=AudioMetadataManager-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
WizardStyle=modern

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el escritorio"; GroupDescription: "Accesos directos adicionales:"; Flags: unchecked

[Files]
Source: "{#MyPublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Iniciar {#MyAppName}"; Flags: nowait postinstall skipifsilent
