using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosSystem
{
    public partial class Maintenance : Form
    {
        private readonly string sourceDbPath = Path.Combine(Application.StartupPath, "DATABASE", "PosDB.db");
        private readonly string logFilePath = Path.Combine(Application.StartupPath, $"maintenance_{DateTime.Now:yyyyMMdd}.log");

        // Pending restore files (used before restart)
        private readonly string pendingRestorePath = Path.Combine(Application.StartupPath, "DATABASE", "PosDB.pendingrestore");
        private readonly string pendingRestoreMarkerPath = Path.Combine(Application.StartupPath, "DATABASE", "PosDB.restore.pending");

        private const string DriveFolderName = "PosSystem Backups";
        private const int CloudRetentionDays = 30;
        private const double MinimumRequiredDriveSpaceMb = 500.0;

        private bool isCloudConnected = false;
        private bool isBusy = false;
        private bool isCloudStateLoading = false;
        private string driveFolderId = string.Empty;

        private bool HasInternet() => NetworkInterface.GetIsNetworkAvailable();

        public Maintenance()
        {
            InitializeComponent();
            timerCloud.Interval = 1800000; // 30 minutes
            DGV1.AllowUserToAddRows = false;

            this.Shown += Maintenance_Shown;
            this.Activated += Maintenance_Activated;
        }

        private async void Maintenance_Shown(object sender, EventArgs e)
        {
            await RefreshCloudStateOnOpenAsync();
        }

        private async void Maintenance_Activated(object sender, EventArgs e)
        {
            await RefreshCloudStateOnOpenAsync();
        }

        private void btnBrowseExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "POS Backup (*.posbk)|*.posbk";
                sfd.DefaultExt = "posbk";
                sfd.AddExtension = true;
                sfd.FileName = $"POS_Backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.posbk";

                if (sfd.ShowDialog() == DialogResult.OK)
                    txtExportPath.Text = sfd.FileName;
            }
        }

        private async void btnExecuteBackup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtExportPath.Text))
                return;

            string exportPath = txtExportPath.Text.Trim();
            if (string.IsNullOrWhiteSpace(Path.GetExtension(exportPath)))
                exportPath += ".posbk";

            btnExecuteBackup.Enabled = false;
            ProgressBar1.Style = ProgressBarStyle.Marquee;
            lblStatus.Text = "Backing up database...";

            try
            {
                string folder = Path.GetDirectoryName(exportPath);
                if (!string.IsNullOrWhiteSpace(folder))
                    Directory.CreateDirectory(folder);

                bool success = await Task.Run(() => CreateSQLiteBackup(sourceDbPath, exportPath));
                if (!success)
                    throw new Exception("Backup file could not be created or validated.");

                long sizeInBytes = new FileInfo(exportPath).Length;
                string readableSize = sizeInBytes < 1024 * 1024
                    ? $"{(sizeInBytes / 1024.0):F2} KB"
                    : $"{(sizeInBytes / (1024.0 * 1024.0)):F2} MB";

                ProgressBar1.Style = ProgressBarStyle.Blocks;
                ProgressBar1.Value = 100;
                lblStatus.Text = "Backup Successful!";

                MessageBox.Show(
                    $"Database backed up successfully!\nSize: {readableSize}",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogError("BACKUP_ERROR", ex);
                Program.SilentLog(ex, "Maintenance_Backup");
                MessageBox.Show("Backup Failed. See daily log for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ProgressBar1.Style = ProgressBarStyle.Blocks;
                ProgressBar1.Value = 0;
                lblStatus.Text = "Backup Failed";
            }
            finally
            {
                btnExecuteBackup.Enabled = true;
            }
        }

        private async Task<bool> UploadBackupToDriveAsync(DriveService service, string folderId)
        {
            string tempBackupPath = Path.Combine(
                Path.GetTempPath(),
                $"PosDB_GDrive_{DateTime.Now:yyyyMMdd_HHmmss_fff}.db");

            try
            {
                if (service == null || string.IsNullOrWhiteSpace(folderId))
                    return false;

                if (!CreateSQLiteBackup(sourceDbPath, tempBackupPath))
                    return false;

                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = GetDriveBackupFileName(),
                    MimeType = "application/x-sqlite3",
                    Parents = new List<string> { folderId }
                };

                using (var stream = new FileStream(tempBackupPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var request = service.Files.Create(fileMetadata, stream, fileMetadata.MimeType);

                    request.ProgressChanged += progress =>
                    {
                        try
                        {
                            if (progress.Status == UploadStatus.Failed && progress.Exception != null)
                                Program.SilentLog(progress.Exception, "Maintenance_DriveUploadProgress");
                        }
                        catch
                        {
                            // stay silent
                        }
                    };

                    var result = await request.UploadAsync();
                    return result.Status == UploadStatus.Completed;
                }
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_UploadBackupToDriveAsync");
                return false;
            }
            finally
            {
                if (File.Exists(tempBackupPath))
                {
                    try { File.Delete(tempBackupPath); } catch { }
                }
            }
        }

        private async void btnExecuteRestore_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtImportPath.Text) || !File.Exists(txtImportPath.Text))
                return;

            string importPath = txtImportPath.Text.Trim();

            try
            {
                lblStatus.Text = "Verifying backup file...";

                bool isValid = await Task.Run(() => ValidateSQLiteFile(importPath));
                if (!isValid)
                {
                    MessageBox.Show("Selected backup is not a valid SQLite backup file.", "Invalid Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Invalid Backup";
                    return;
                }

                if (MessageBox.Show(
                    "Current data will be replaced after restart.\nThe restore will be prepared now and applied when the app starts again.\n\nContinue?",
                    "Confirm Restore",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return;
                }

                btnExecuteRestore.Enabled = false;
                lblStatus.Text = "Preparing restore and restart...";

                bool queued = await QueueLocalRestoreForRestartAsync(importPath);
                if (!queued)
                {
                    MessageBox.Show("Restore preparation failed. No changes were made.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Restore Preparation Failed";
                    return;
                }

                Program.SilentLog(new Exception("Local restore queued for restart"), "Maintenance_RestoreLocal_Queued");

                MessageBox.Show(
                    "Restore has been prepared.\nThe application will now restart to complete the restore.",
                    "Restart Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                RestartApplicationForRestore();
            }
            catch (Exception ex)
            {
                LogError("RESTORE_ERROR", ex);
                Program.SilentLog(ex, "Maintenance_RestoreLocal");
                MessageBox.Show($"Restore Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Restore Failed";
            }
            finally
            {
                btnExecuteRestore.Enabled = true;
            }
        }

        private void btnBrowseImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "POS Backup (*.posbk)|*.posbk|SQLite Database (*.db)|*.db|All Files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                    txtImportPath.Text = ofd.FileName;
            }
        }

        private void LogError(string type, Exception ex)
        {
            try
            {
                string logEntry =
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {type}: {ex.Message}{Environment.NewLine}" +
                    $"{ex.StackTrace}{Environment.NewLine}{new string('-', 30)}{Environment.NewLine}";

                File.AppendAllText(logFilePath, logEntry);
            }
            catch
            {
                // stay silent
            }
        }

        #region --- UTILS ---

        private async Task<bool> HasInternetReal()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(4);
                    using (HttpResponseMessage response = await client.GetAsync("https://www.google.com/generate_204"))
                    {
                        return response.IsSuccessStatusCode;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private void SetCloudStatus(string text, Color color)
        {
            lblCloudStatus.Text = text;
            lblCloudStatus.ForeColor = color;
        }

        private void ClearDriveGrid()
        {
            try
            {
                DGV1.Rows.Clear();
                DGV1.Refresh();
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_ClearDriveGrid");
            }
        }

        private async Task RefreshCloudStateOnOpenAsync()
        {
            if (isCloudStateLoading || isBusy)
                return;

            isCloudStateLoading = true;

            try
            {
                string tokenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PosSystem");
                bool hasToken = false;

                try
                {
                    hasToken = Directory.Exists(tokenPath) && Directory.EnumerateFileSystemEntries(tokenPath).Any();
                }
                catch { }

                if (!hasToken)
                {
                    isCloudConnected = false;
                    driveFolderId = string.Empty;
                    btnConnectDrive.Text = "Connect";
                    SetCloudStatus("Status: Disconnected", Color.Black);
                    ClearDriveGrid();
                    return;
                }

                if (!await HasInternetReal())
                {
                    isCloudConnected = true;
                    btnConnectDrive.Text = "Disconnect";
                    SetCloudStatus("Status: Connected (Offline)", Color.DarkGoldenrod);
                    return;
                }

                var handler = new GoogleDriveHandler();
                var service = await handler.GetServiceAsync();
                driveFolderId = await GetOrCreateDriveFolderIdAsync(service);

                await RefreshDriveGridInternalAsync(service, driveFolderId);

                isCloudConnected = true;
                btnConnectDrive.Text = "Disconnect";
                SetCloudStatus("Status: Connected", Color.DarkGreen);
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_RefreshCloudStateOnOpen");
            }
            finally
            {
                isCloudStateLoading = false;
            }
        }

        private string GetDriveBackupFileName()
        {
            string machine = Environment.MachineName;
            foreach (char c in Path.GetInvalidFileNameChars())
                machine = machine.Replace(c.ToString(), string.Empty);

            if (string.IsNullOrWhiteSpace(machine))
                machine = "PC";

            return $"PosDB_{machine}_{DateTime.Now:yyyyMMdd_HHmmss}.db";
        }

        private bool HasBlockingFormsOpen()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form == null || form.IsDisposed)
                    continue;

                if (!form.Visible)
                    continue;

                if (form == this)
                    continue;

                if (this.Owner != null && form == this.Owner)
                    continue;

                string formName = form.Name ?? string.Empty;

                if (formName.Equals("Form1", StringComparison.OrdinalIgnoreCase) ||
                    formName.Equals("frmMain", StringComparison.OrdinalIgnoreCase) ||
                    formName.Equals("MainForm", StringComparison.OrdinalIgnoreCase) ||
                    formName.Equals("Maintenance", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private bool NeedsBrowserAuth()
        {
            try
            {
                string tokenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PosSystem");
                return !Directory.Exists(tokenPath) || !Directory.EnumerateFileSystemEntries(tokenPath).Any();
            }
            catch
            {
                return true;
            }
        }

        private bool TryLaunchDefaultBrowser()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://accounts.google.com",
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_DefaultBrowserLaunch");
                return false;
            }
        }

        private string EscapeDriveQueryValue(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'");
        }

        private async Task<string> GetOrCreateDriveFolderIdAsync(DriveService service)
        {
            if (!string.IsNullOrWhiteSpace(driveFolderId))
                return driveFolderId;

            string safeFolderName = EscapeDriveQueryValue(DriveFolderName);

            var listRequest = service.Files.List();
            listRequest.Q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{safeFolderName}' and trashed = false";
            listRequest.Fields = "files(id, name)";
            listRequest.PageSize = 10;

            var listResult = await listRequest.ExecuteAsync();
            var existingFolder = listResult.Files?.FirstOrDefault();

            if (existingFolder != null)
            {
                driveFolderId = existingFolder.Id;
                return driveFolderId;
            }

            var folderMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = DriveFolderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            var createRequest = service.Files.Create(folderMetadata);
            createRequest.Fields = "id, name";

            var createdFolder = await createRequest.ExecuteAsync();
            driveFolderId = createdFolder.Id;
            return driveFolderId;
        }

        private async Task<double> GetAvailableDriveSpaceMbAsync(DriveService service)
        {
            try
            {
                var aboutRequest = service.About.Get();
                aboutRequest.Fields = "storageQuota(limit,usage)";
                var about = await aboutRequest.ExecuteAsync();

                if (about?.StorageQuota == null)
                    return -1;

                if (about.StorageQuota.Limit.HasValue && about.StorageQuota.Usage.HasValue)
                {
                    long availableBytes = about.StorageQuota.Limit.Value - about.StorageQuota.Usage.Value;
                    return availableBytes / (1024.0 * 1024.0);
                }

                return -1;
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_DriveQuota");
                return -1;
            }
        }


        private void DeleteSidecarFiles(string basePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(basePath))
                    return;

                if (File.Exists(basePath + "-wal"))
                    File.Delete(basePath + "-wal");

                if (File.Exists(basePath + "-shm"))
                    File.Delete(basePath + "-shm");
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_DeleteSidecarFiles");
            }
        }

        private bool CreateSQLiteBackup(string sourcePath, string backupPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                    return false;

                string backupFolder = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrWhiteSpace(backupFolder) && !Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);

                try
                {
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                }
                catch { }

                DeleteSidecarFiles(backupPath);

                string sourceConString = $"Data Source={sourcePath};Version=3;Busy Timeout=5000;";
                string destinationConString = $"Data Source={backupPath};Version=3;Busy Timeout=5000;";

                using (SQLiteConnection source = new SQLiteConnection(sourceConString))
                using (SQLiteConnection destination = new SQLiteConnection(destinationConString))
                {
                    source.Open();

                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA wal_checkpoint(FULL);", source))
                        cmd.ExecuteNonQuery();

                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA wal_checkpoint(TRUNCATE);", source))
                        cmd.ExecuteNonQuery();

                    destination.Open();

                    source.BackupDatabase(destination, "main", "main", -1, null, 0);

                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA journal_mode=DELETE;", destination))
                        cmd.ExecuteScalar();
                }

                DeleteSidecarFiles(backupPath);

                return ValidateSQLiteFile(backupPath);
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_CreateSQLiteBackup");
                return false;
            }
        }

        private bool ValidateSQLiteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    return false;

                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length <= 0)
                    return false;

                using (SQLiteConnection test = new SQLiteConnection($"Data Source={filePath};Version=3;BusyTimeout=3000;"))
                {
                    test.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA integrity_check;", test))
                    {
                        object result = cmd.ExecuteScalar();
                        return result != null &&
                               result.ToString().Equals("ok", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_ValidateSQLiteFile");
                return false;
            }
        }

        private async Task<bool> QueueLocalRestoreForRestartAsync(string backupFilePath)
        {
            string tempPath = pendingRestorePath + ".tmp";

            try
            {
                if (!ValidateSQLiteFile(backupFilePath))
                    return false;

                string dbFolder = Path.GetDirectoryName(sourceDbPath);
                if (!string.IsNullOrWhiteSpace(dbFolder) && !Directory.Exists(dbFolder))
                    Directory.CreateDirectory(dbFolder);

                try
                {
                    if (File.Exists(pendingRestorePath))
                        File.Delete(pendingRestorePath);
                }
                catch { }

                try
                {
                    if (File.Exists(pendingRestoreMarkerPath))
                        File.Delete(pendingRestoreMarkerPath);
                }
                catch { }

                File.Copy(backupFilePath, tempPath, true);

                if (!ValidateSQLiteFile(tempPath))
                    return false;

                if (File.Exists(pendingRestorePath))
                    File.Replace(tempPath, pendingRestorePath, null);
                else
                    File.Move(tempPath, pendingRestorePath);

                File.WriteAllText(pendingRestoreMarkerPath, DateTime.Now.ToString("O"));
                return true;
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_QueueLocalRestoreForRestartAsync");
                return false;
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

        private async Task<bool> QueueDriveRestoreForRestartAsync(string fileId)
        {
            string tempDownloadPath = pendingRestorePath + ".download";

            try
            {
                if (string.IsNullOrWhiteSpace(fileId))
                    return false;

                string dbFolder = Path.GetDirectoryName(sourceDbPath);
                if (!string.IsNullOrWhiteSpace(dbFolder) && !Directory.Exists(dbFolder))
                    Directory.CreateDirectory(dbFolder);

                try
                {
                    if (File.Exists(pendingRestorePath))
                        File.Delete(pendingRestorePath);
                }
                catch { }

                try
                {
                    if (File.Exists(pendingRestoreMarkerPath))
                        File.Delete(pendingRestoreMarkerPath);
                }
                catch { }

                var handler = new GoogleDriveHandler();
                var service = await handler.GetServiceAsync();
                var request = service.Files.Get(fileId);

                using (var stream = new FileStream(tempDownloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await request.DownloadAsync(stream);
                    await stream.FlushAsync();
                }

                if (!ValidateSQLiteFile(tempDownloadPath))
                    return false;

                if (File.Exists(pendingRestorePath))
                    File.Replace(tempDownloadPath, pendingRestorePath, null);
                else
                    File.Move(tempDownloadPath, pendingRestorePath);

                File.WriteAllText(pendingRestoreMarkerPath, DateTime.Now.ToString("O"));
                return true;
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_QueueDriveRestoreForRestartAsync");
                return false;
            }
            finally
            {
                if (File.Exists(tempDownloadPath))
                {
                    try { File.Delete(tempDownloadPath); } catch { }
                }
            }
        }

        private void RestartApplicationForRestore()
        {
            try
            {
                DBConnection.CloseAll();
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_RestartApplicationForRestore_CloseAll");
            }

            try
            {
                SQLiteConnection.ClearAllPools();
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_RestartApplicationForRestore_ClearPools");
            }

            try
            {
                Application.Restart();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_RestartApplicationForRestore");
                Application.Exit();
            }
        }

        #endregion

        #region --- SMART SYNC & RETENTION LOGIC ---

        private async Task RunSmartSync(bool isManual)
        {
            if (isBusy)
                return;

            isBusy = true;

            try
            {
                if (!await HasInternetReal())
                {
                    if (isManual)
                        MessageBox.Show("No working internet connection detected.", "Cloud Backup", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                var handler = new GoogleDriveHandler();
                var service = await handler.GetServiceAsync();
                string folderId = await GetOrCreateDriveFolderIdAsync(service);

                double availableMb = await GetAvailableDriveSpaceMbAsync(service);
                if (availableMb >= 0 && availableMb < MinimumRequiredDriveSpaceMb)
                {
                    string msg = $"Google Drive needs at least {MinimumRequiredDriveSpaceMb:F0} MB free space.\nAvailable: {availableMb:F2} MB";
                    if (isManual)
                        MessageBox.Show(msg, "Insufficient Drive Space", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    Program.SilentLog(new Exception(msg), "Maintenance_DriveSpace");
                    return;
                }

                await CleanupOldBackups(service, folderId, CloudRetentionDays);

                if (File.Exists(sourceDbPath))
                {
                    DateTime lastWrite = File.GetLastWriteTime(sourceDbPath);
                    if (!isManual && DateTime.Now.Subtract(lastWrite).TotalMinutes > 30)
                        return;
                }

                bool success = await UploadBackupToDriveAsync(service, folderId);

                if (isManual)
                {
                    if (success)
                    {
                        MessageBox.Show("Cloud Backup Successful!", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Upload failed. Check your internet, Google account access, or Drive free space.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Program.SilentLog(new Exception("Manual Upload Failed"), "Maintenance_ManualSync");
                    }

                    await RefreshDriveGridInternalAsync(service, folderId);
                }
                else
                {
                    if (success && this.Visible && isCloudConnected)
                        await RefreshDriveGridInternalAsync(service, folderId);
                }
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_SmartSync_Task");
                LogError("SMART_SYNC_ERROR", ex);
            }
            finally
            {
                isBusy = false;
            }
        }

        private async Task CleanupOldBackups(DriveService service, string folderId, int days)
        {
            try
            {
                var listRequest = service.Files.List();
                listRequest.Q = $"'{folderId}' in parents and trashed = false";
                listRequest.Fields = "files(id, createdTime)";
                listRequest.PageSize = 1000;

                var result = await listRequest.ExecuteAsync();
                DateTime cutOffDate = DateTime.Now.AddDays(-days);

                if (result.Files != null)
                {
                    foreach (var file in result.Files)
                    {
                        if (file.CreatedTimeDateTimeOffset.HasValue &&
                            file.CreatedTimeDateTimeOffset.Value.LocalDateTime < cutOffDate)
                        {
                            await service.Files.Delete(file.Id).ExecuteAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Maintenance_RetentionCleanup");
            }
        }

        private async Task RefreshDriveGridInternalAsync(DriveService service, string folderId)
        {
            try
            {
                DGV1.Rows.Clear();

                var listRequest = service.Files.List();
                listRequest.Q = $"'{folderId}' in parents and trashed = false";
                listRequest.Fields = "files(id, name, createdTime, size)";
                listRequest.PageSize = 1000;
                listRequest.OrderBy = "createdTime desc";

                var result = await listRequest.ExecuteAsync();

                if (result.Files != null)
                {
                    foreach (var file in result.Files)
                    {
                        double sizeMb = (file.Size ?? 0) / (1024.0 * 1024.0);
                        string displayDate = file.CreatedTimeDateTimeOffset?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "N/A";

                        DGV1.Rows.Add(
                            file.Id,
                            file.Name,
                            displayDate,
                            sizeMb.ToString("F2") + " MB",
                            "Restore");
                    }
                }

                isCloudConnected = true;
                btnConnectDrive.Text = "Disconnect";
                SetCloudStatus("Status: Connected", Color.DarkGreen);
                DGV1.Refresh();
            }
            catch (Exception ex)
            {
                SetCloudStatus("Status: Sync Standby", Color.Black);
                Program.SilentLog(ex, "Maintenance_GridRefreshInternal");
            }
        }

        private async Task RefreshDriveGridAsync()
        {
            if (!await HasInternetReal())
                return;

            try
            {
                var handler = new GoogleDriveHandler();
                var service = await handler.GetServiceAsync();
                string folderId = await GetOrCreateDriveFolderIdAsync(service);
                await RefreshDriveGridInternalAsync(service, folderId);
            }
            catch (Exception ex)
            {
                SetCloudStatus("Status: Sync Standby", Color.Black);
                Program.SilentLog(ex, "Maintenance_GridRefresh");
            }
        }

        #endregion

        #region --- BUTTON ACTIONS ---

        private async void btnConnectDrive_Click(object sender, EventArgs e)
        {
            if (isBusy)
                return;

            if (isCloudConnected)
            {
                if (MessageBox.Show("Do you want to disconnect Google Drive?", "Disconnect", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string tokenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PosSystem");
                    try
                    {
                        if (Directory.Exists(tokenPath))
                            Directory.Delete(tokenPath, true);
                    }
                    catch (Exception ex)
                    {
                        Program.SilentLog(ex, "Maintenance_Disconnect_DeleteToken");
                    }

                    driveFolderId = string.Empty;
                    isCloudConnected = false;
                    timerCloud.Stop();
                    chkAutoSync.Checked = false;
                    ClearDriveGrid();
                    btnConnectDrive.Text = "Connect";
                    SetCloudStatus("Status: Disconnected", Color.Black);
                }

                return;
            }

            if (!await HasInternetReal())
            {
                MessageBox.Show("No working internet connection detected.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");
            if (!File.Exists(credPath))
            {
                MessageBox.Show("credentials.json is missing from the application folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                isBusy = true;
                btnConnectDrive.Enabled = false;

                SetCloudStatus("Status: Preparing Google Drive login...", Color.DarkBlue);

                if (NeedsBrowserAuth())
                {
                    bool browserOpened = TryLaunchDefaultBrowser();
                    if (!browserOpened)
                    {
                        MessageBox.Show(
                            "Windows could not open a default browser.\nPlease set a default browser and try again.",
                            "Browser Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        SetCloudStatus("Status: Browser Error", Color.DarkRed);
                        return;
                    }
                }

                SetCloudStatus("Status: Connecting to Google Drive...", Color.DarkBlue);

                var handler = new GoogleDriveHandler();
                var service = await handler.GetServiceAsync();
                driveFolderId = await GetOrCreateDriveFolderIdAsync(service);

                isCloudConnected = true;
                btnConnectDrive.Text = "Disconnect";
                SetCloudStatus("Status: Connected!", Color.DarkGreen);

                await RefreshDriveGridInternalAsync(service, driveFolderId);
            }
            catch (Exception ex)
            {
                isCloudConnected = false;
                driveFolderId = string.Empty;
                btnConnectDrive.Text = "Connect";
                SetCloudStatus("Status: Auth Failed", Color.DarkRed);

                Program.SilentLog(ex, "Maintenance_ConnectDrive");
                MessageBox.Show(
                    $"Google Drive connection failed.\n\nType: {ex.GetType().Name}\nMessage: {ex.Message}",
                    "Google Drive Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnConnectDrive.Enabled = true;
                isBusy = false;
            }
        }

        private async void btnSyncNow_Click(object sender, EventArgs e)
        {
            if (!isCloudConnected)
            {
                MessageBox.Show("Please connect to Google Drive first.", "Action Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnSyncNow.Enabled = false;
            SetCloudStatus("Status: Syncing...", Color.DarkBlue);

            await RunSmartSync(true);

            SetCloudStatus("Status: Last Sync " + DateTime.Now.ToString("HH:mm"), Color.DarkGreen);
            btnSyncNow.Enabled = true;
        }

        private async void btnRefreshGrid_Click(object sender, EventArgs e)
        {
            await RefreshDriveGridAsync();
        }

        #endregion

        #region --- GRID RESTORE ---

        private async void DGV1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || DGV1.Columns[e.ColumnIndex].Name != "colAction")
                return;

            string fileId = Convert.ToString(DGV1.Rows[e.RowIndex].Cells["colID"].Value);
            string fileName = Convert.ToString(DGV1.Rows[e.RowIndex].Cells["colName"].Value);

            if (string.IsNullOrWhiteSpace(fileId))
                return;

            if (MessageBox.Show(
                $"The selected cloud backup will be restored after restart:\n{fileName}\n\nCurrent data will be replaced.\nContinue?",
                "Confirm Restore",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            SetCloudStatus("Status: Preparing restore...", Color.DarkBlue);

            bool success = await QueueDriveRestoreForRestartAsync(fileId);

            if (success)
            {
                Program.SilentLog(new Exception($"Drive restore queued: {fileName}"), "Maintenance_Restore_Queued");

                MessageBox.Show(
                    "Restore has been prepared.\nThe application will now restart to complete the restore.",
                    "Restart Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                RestartApplicationForRestore();
            }
            else
            {
                MessageBox.Show("Restore preparation failed. No changes were made.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.SilentLog(new Exception("Restore Queue Failure"), "Maintenance_Restore_Fail");
                SetCloudStatus("Status: Restore Preparation Failed", Color.DarkRed);
            }
        }

        #endregion

        private void chkAutoSync_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoSync.Checked)
            {
                if (!isCloudConnected)
                {
                    MessageBox.Show("Please connect to Google Drive first before enabling Auto-Sync.", "Cloud Connection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    chkAutoSync.Checked = false;
                    return;
                }

                timerCloud.Start();
                SetCloudStatus("Status: Auto-Sync Active", Color.DarkGreen);
            }
            else
            {
                timerCloud.Stop();
                SetCloudStatus("Status: Auto-Sync Disabled", Color.Black);
            }
        }

        private async void timerCloud_Tick(object sender, EventArgs e)
        {
            await RunSmartSync(false);
            SetCloudStatus("Status: Last Auto-Sync " + DateTime.Now.ToString("HH:mm"), Color.DarkGreen);
        }

        // =========================================================================
        // PLACEHOLDER HANDLERS (Designer-safe)
        // =========================================================================

        private void txtImportPath_TextChanged(object sender, EventArgs e) { }
        private void chkCompress_CheckedChanged(object sender, EventArgs e) { }
        private void lblLastBackup_Click(object sender, EventArgs e) { }
        private void lblStatus_Click(object sender, EventArgs e) { }
        private void ProgressBar1_Click(object sender, EventArgs e) { }
        private void txtExportPath_TextChanged(object sender, EventArgs e) { }
        private void lblCloudStatus_Click(object sender, EventArgs e) { }

        private void DGV1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            DGV1_CellContentClick(sender, e);
        }
    }
}