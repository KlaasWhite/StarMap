[Setup]
AppName=StarMap
AppVersion={#AppVersion}
AppId={{A6C53918-1F1B-4C7B-AF34-63C696A3E822}}
DefaultDirName={pf}\StarMap
DisableDirPage=no
DisableProgramGroupPage=yes
OutputDir=dist
OutputBaseFilename={#OutputName}
Compression=lzma
SolidCompression=yes
Uninstallable=yes

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{autoprograms}\StarMap"; Filename: "{app}\StarMap.exe"
Name: "{autodesktop}\StarMap"; Filename: "{app}\StarMap.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create desktop icon"; Flags: unchecked

[UninstallDelete]
Type: files; Name: "{app}\StarMapConfig.json"

[Code]
var 
    DirPage: TInputDirWizardPage; 
    SelectedInstallDir: String; 
    InstallDir: String; 

procedure InitializeWizard; 
begin 
    DirPage := CreateInputDirPage( 
        wpSelectDir, 
        'KSA Install Location', 
        'Please select the KSA install location (folder with KSA.dll)', 
        'This will be used to launch KSA.', 
        False, 
        'New Folder' 
    ); 
    DirPage.Add(''); 
    DirPage.Values[0] := ''; 
end; 

procedure CurStepChanged(CurStep: TSetupStep); 
var 
    ConfigFilePath: String; 
    JSONText: AnsiString; 
    SaveOK: Boolean; 
    ReadBack: AnsiString; 
    EscapedDir: String; 
begin 
    InstallDir := ExpandConstant('{app}'); 
    ConfigFilePath := AddBackslash(InstallDir) + 'StarMapConfig.json'; 

    if (CurStep = ssPostInstall) and (not FileExists(ConfigFilePath)) then 
    begin 
        if (DirPage <> nil) and (DirPage.Values[0] <> '') then 
            SelectedInstallDir := DirPage.Values[0] 
        else 
            SelectedInstallDir := WizardDirValue; 

        SelectedInstallDir := ExpandConstant(SelectedInstallDir); 

        
        
        EscapedDir := SelectedInstallDir; 
        StringChangeEx(EscapedDir, '\', '\\', True); 
        
        JSONText := '{' + '"GameLocation": "' + EscapedDir + '",' + '"RepositoryLocation": ""' + '}'; 
        SaveOK := SaveStringToFile(ConfigFilePath, JSONText, False);
    end; 
end;