; Enterprise Deployment Framework - Database-aware Inno Setup template
#define MyAppName "Product"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Company"
#define MyAppExeName "Product.AppLauncher.exe"

[Setup]
AppId={{7B2A3C90-82CD-4A54-BE88-EDF000000001}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppPublisher}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename={#MyAppName}-Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupLogging=yes

[Files]
Source: "..\..\artifacts\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\..\artifacts\tools\Product.ConfigTool.exe"; DestDir: "{app}\Tools"; Flags: ignoreversion
Source: "..\..\artifacts\tools\Product.DbConnectionTester.exe"; DestDir: "{app}\Tools"; Flags: ignoreversion
Source: "..\..\artifacts\tools\Product.Diagnostics.exe"; DestDir: "{app}\Tools"; Flags: ignoreversion
Source: "..\..\artifacts\tools\Product.Repair.exe"; DestDir: "{app}\Tools"; Flags: ignoreversion
Source: "..\..\artifacts\tools\Product.BackupRestore.exe"; DestDir: "{app}\Tools"; Flags: ignoreversion
Source: "..\..\artifacts\tools\Product.MigrationRunner.exe"; DestDir: "{app}\Tools"; Flags: ignoreversion
Source: "..\..\artifacts\tools\Product.Update.exe"; DestDir: "{app}\Tools"; Flags: ignoreversion
Source: "prerequisites\*"; DestDir: "{tmp}\prerequisites"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional tasks:"
Name: "startup"; Description: "Start the application when I sign in"; GroupDescription: "Additional tasks:"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Tasks: startup; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\{#MyAppExeName}"; Parameters: "--shutdown"; Flags: runhidden skipifdoesntexist; RunOnceId: "StopApplication"

[Code]
var
  DatabasePage: TWizardPage;
  LocalRadio, ServerRadio: TNewRadioButton;
  ServerLabel, DatabaseLabel, AuthLabel, UserLabel, PasswordLabel, TestStatus: TNewStaticText;
  ServerEdit, DatabaseEdit, UserEdit: TNewEdit;
  PasswordEdit: TPasswordEdit;
  AuthCombo: TNewComboBox;
  TestButton: TNewButton;
  ConnectionTestSucceeded: Boolean;

procedure UpdateDatabaseControls(Sender: TObject);
var ServerMode, SqlAuthentication: Boolean;
begin
  ServerMode := ServerRadio.Checked;
  SqlAuthentication := ServerMode and (AuthCombo.ItemIndex = 1);
  ServerLabel.Visible := ServerMode; ServerEdit.Visible := ServerMode;
  AuthLabel.Visible := ServerMode; AuthCombo.Visible := ServerMode;
  UserLabel.Visible := SqlAuthentication; UserEdit.Visible := SqlAuthentication;
  PasswordLabel.Visible := SqlAuthentication; PasswordEdit.Visible := SqlAuthentication;
  ConnectionTestSucceeded := False;
  TestStatus.Caption := 'Connection has not been tested.';
end;

procedure WriteRequestIni(const Path: String; IncludeTarget: Boolean);
var DatabaseTypeValue, AuthenticationValue, DataPath, Content: String;
begin
  if LocalRadio.Checked then DatabaseTypeValue := 'Local' else DatabaseTypeValue := 'InternalServer';
  if AuthCombo.ItemIndex = 1 then AuthenticationValue := 'SqlServer' else AuthenticationValue := 'Windows';
  DataPath := ExpandConstant('{commonappdata}\{#MyAppPublisher}\{#MyAppName}\Data');
  Content :=
    'DatabaseType=' + DatabaseTypeValue + #13#10 +
    'LocalProvider=LocalDB' + #13#10 +
    'DataPath=' + DataPath + #13#10 +
    'ServerName=' + ServerEdit.Text + #13#10 +
    'DatabaseName=' + DatabaseEdit.Text + #13#10 +
    'AuthenticationType=' + AuthenticationValue + #13#10 +
    'UserName=' + UserEdit.Text + #13#10 +
    'Password=' + PasswordEdit.Text + #13#10 +
    'TrustServerCertificate=true' + #13#10 +
    'TimeoutSeconds=8' + #13#10;
  if IncludeTarget then
    Content := Content +
      'TargetPath=' + ExpandConstant('{commonappdata}\{#MyAppPublisher}\{#MyAppName}\Config\appsettings.runtime.json') + #13#10 +
      'CompanyName={#MyAppPublisher}' + #13#10 +
      'ApplicationName={#MyAppName}' + #13#10 +
      'Port=5080' + #13#10;
  SaveStringToFile(Path, Content, False);
end;

procedure TestConnectionClick(Sender: TObject);
var RequestPath, ResultPath, Parameters: String; ResultCode: Integer;
begin
  RequestPath := ExpandConstant('{tmp}\edf-db-request.ini');
  ResultPath := ExpandConstant('{tmp}\edf-db-result.json');
  WriteRequestIni(RequestPath, False);
  Parameters := '--request-ini "' + RequestPath + '" --result-json "' + ResultPath + '" --silent';
  if Exec(ExpandConstant('{app}\Tools\Product.DbConnectionTester.exe'), Parameters, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0) then
  begin
    ConnectionTestSucceeded := True;
    TestStatus.Caption := 'Connection established successfully.';
    MsgBox('Connection established successfully.', mbInformation, MB_OK);
  end
  else
  begin
    ConnectionTestSucceeded := False;
    TestStatus.Caption := 'Database connection failed.';
    MsgBox('Database connection failed. Check credentials, server, instance, or LocalDB availability.', mbError, MB_OK);
  end;
  DeleteFile(RequestPath); DeleteFile(ResultPath);
end;

procedure InitializeWizard;
begin
  DatabasePage := CreateCustomPage(wpSelectTasks, 'Database Connection', 'Select and validate the database.');
  LocalRadio := TNewRadioButton.Create(DatabasePage); LocalRadio.Parent := DatabasePage.Surface; LocalRadio.Caption := 'Local database on this computer'; LocalRadio.Checked := True; LocalRadio.Top := 8; LocalRadio.OnClick := @UpdateDatabaseControls;
  ServerRadio := TNewRadioButton.Create(DatabasePage); ServerRadio.Parent := DatabasePage.Surface; ServerRadio.Caption := 'Internal SQL Server / company network'; ServerRadio.Top := 34; ServerRadio.OnClick := @UpdateDatabaseControls;
  ServerLabel := TNewStaticText.Create(DatabasePage); ServerLabel.Parent := DatabasePage.Surface; ServerLabel.Caption := 'Server / Instance:'; ServerLabel.Top := 72;
  ServerEdit := TNewEdit.Create(DatabasePage); ServerEdit.Parent := DatabasePage.Surface; ServerEdit.Text := '.\SQLEXPRESS'; ServerEdit.Top := 90; ServerEdit.Width := DatabasePage.SurfaceWidth;
  DatabaseLabel := TNewStaticText.Create(DatabasePage); DatabaseLabel.Parent := DatabasePage.Surface; DatabaseLabel.Caption := 'Database name:'; DatabaseLabel.Top := 122;
  DatabaseEdit := TNewEdit.Create(DatabasePage); DatabaseEdit.Parent := DatabasePage.Surface; DatabaseEdit.Text := '{#MyAppName}DB'; DatabaseEdit.Top := 140; DatabaseEdit.Width := DatabasePage.SurfaceWidth;
  AuthLabel := TNewStaticText.Create(DatabasePage); AuthLabel.Parent := DatabasePage.Surface; AuthLabel.Caption := 'Authentication:'; AuthLabel.Top := 172;
  AuthCombo := TNewComboBox.Create(DatabasePage); AuthCombo.Parent := DatabasePage.Surface; AuthCombo.Style := csDropDownList; AuthCombo.Items.Add('Windows Authentication'); AuthCombo.Items.Add('SQL Server Authentication'); AuthCombo.ItemIndex := 0; AuthCombo.Top := 190; AuthCombo.Width := DatabasePage.SurfaceWidth; AuthCombo.OnChange := @UpdateDatabaseControls;
  UserLabel := TNewStaticText.Create(DatabasePage); UserLabel.Parent := DatabasePage.Surface; UserLabel.Caption := 'User name:'; UserLabel.Top := 222;
  UserEdit := TNewEdit.Create(DatabasePage); UserEdit.Parent := DatabasePage.Surface; UserEdit.Top := 240; UserEdit.Width := (DatabasePage.SurfaceWidth div 2) - 4;
  PasswordLabel := TNewStaticText.Create(DatabasePage); PasswordLabel.Parent := DatabasePage.Surface; PasswordLabel.Caption := 'Password:'; PasswordLabel.Top := 222; PasswordLabel.Left := (DatabasePage.SurfaceWidth div 2) + 4;
  PasswordEdit := TPasswordEdit.Create(DatabasePage); PasswordEdit.Parent := DatabasePage.Surface; PasswordEdit.Top := 240; PasswordEdit.Left := (DatabasePage.SurfaceWidth div 2) + 4; PasswordEdit.Width := (DatabasePage.SurfaceWidth div 2) - 4;
  TestButton := TNewButton.Create(DatabasePage); TestButton.Parent := DatabasePage.Surface; TestButton.Caption := 'Test Connection'; TestButton.Top := 278; TestButton.Width := 120; TestButton.OnClick := @TestConnectionClick;
  TestStatus := TNewStaticText.Create(DatabasePage); TestStatus.Parent := DatabasePage.Surface; TestStatus.Caption := 'Connection has not been tested.'; TestStatus.Top := 284; TestStatus.Left := 132;
  UpdateDatabaseControls(nil);
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = DatabasePage.ID then
  begin
    if Trim(DatabaseEdit.Text) = '' then begin MsgBox('Database name is required.', mbError, MB_OK); Result := False; Exit; end;
    if ServerRadio.Checked and (Trim(ServerEdit.Text) = '') then begin MsgBox('Server or instance name is required.', mbError, MB_OK); Result := False; Exit; end;
    if not ConnectionTestSucceeded then Result := MsgBox('The connection test has not succeeded. Continue anyway?', mbConfirmation, MB_YESNO) = IDYES;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var RequestPath, Parameters: String; ResultCode: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    ForceDirectories(ExpandConstant('{commonappdata}\{#MyAppPublisher}\{#MyAppName}\Config'));
    ForceDirectories(ExpandConstant('{commonappdata}\{#MyAppPublisher}\{#MyAppName}\Data'));
    ForceDirectories(ExpandConstant('{commonappdata}\{#MyAppPublisher}\{#MyAppName}\Logs'));
    RequestPath := ExpandConstant('{tmp}\edf-config-request.ini');
    WriteRequestIni(RequestPath, True);
    Parameters := '--apply-installer-ini "' + RequestPath + '" --silent';
    if not Exec(ExpandConstant('{app}\Tools\Product.ConfigTool.exe'), Parameters, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) or (ResultCode <> 0) then
      RaiseException('Application configuration could not be written.');
    DeleteFile(RequestPath);
  end;
end;
