using System;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using System.Data.SQLite;

namespace PosSystem
{
    public static class CloudSyncHelper
    {
        private static readonly string localFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DATABASE", "PosDB.db");

        public static async Task<bool> UploadBackup()
        {
            string tempBackupPath = null;

            try
            {
                if (!File.Exists(localFile))
                    return false;

                tempBackupPath = CreateTempBackupSnapshot();
                if (string.IsNullOrWhiteSpace(tempBackupPath) || !File.Exists(tempBackupPath))
                    return false;

                var handler = new GoogleDriveHandler();
                var service = await handler.GetServiceAsync();

                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = $"PosDB-{DateTime.Now:yyyyMMdd-HHmm}.db",
                    MimeType = "application/x-sqlite3"
                };

                using (var stream = new FileStream(tempBackupPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var request = service.Files.Create(fileMetadata, stream, fileMetadata.MimeType);

                    request.ProgressChanged += progress =>
                    {
                        try
                        {
                            if (progress.Status == UploadStatus.Failed && progress.Exception != null)
                                Program.SilentLog(progress.Exception, "CloudSync_Upload_Stream_Error");
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
                Program.SilentLog(ex, "CloudSync_Upload_Crash");
                return false;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(tempBackupPath) && File.Exists(tempBackupPath))
                {
                    try { File.Delete(tempBackupPath); } catch { }
                }
            }
        }

        public static async Task<bool> RestoreBackup(string fileId)
        {
            string tempDownloadPath = localFile + ".tmp_download";

            try
            {
                if (string.IsNullOrWhiteSpace(fileId))
                    return false;

                string dbFolder = Path.GetDirectoryName(localFile);
                if (!string.IsNullOrWhiteSpace(dbFolder) && !Directory.Exists(dbFolder))
                    Directory.CreateDirectory(dbFolder);

                // Release DB resources before restore
                DBConnection.CloseAll();
                try { SQLiteConnection.ClearAllPools(); } catch { }
                await Task.Delay(1000);

                var handler = new GoogleDriveHandler();
                var service = await handler.GetServiceAsync();
                var request = service.Files.Get(fileId);

                using (var stream = new FileStream(tempDownloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await request.DownloadAsync(stream);
                    await stream.FlushAsync();
                }

                if (!File.Exists(tempDownloadPath))
                    return false;

                if (!ValidateSQLiteFile(tempDownloadPath))
                    return false;

                // Clean SQLite sidecar files
                try
                {
                    if (File.Exists(localFile + "-wal")) File.Delete(localFile + "-wal");
                    if (File.Exists(localFile + "-shm")) File.Delete(localFile + "-shm");
                }
                catch (Exception ex)
                {
                    Program.SilentLog(ex, "CloudSync_Restore_DeleteSidecars");
                }

                if (File.Exists(localFile))
                    File.Replace(tempDownloadPath, localFile, null);
                else
                    File.Move(tempDownloadPath, localFile);

                try
                {
                    if (File.Exists(localFile + "-wal")) File.Delete(localFile + "-wal");
                    if (File.Exists(localFile + "-shm")) File.Delete(localFile + "-shm");
                }
                catch (Exception ex)
                {
                    Program.SilentLog(ex, "CloudSync_Restore_PostReplaceSidecars");
                }

                return true;
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "CloudSync_Restore_Crash");
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

        private static string CreateTempBackupSnapshot()
        {
            string tempBackupPath = Path.Combine(
                Path.GetTempPath(),
                $"PosDB_Upload_{DateTime.Now:yyyyMMdd_HHmmss_fff}.db");

            try
            {
                using (var source = new SQLiteConnection(DBConnection.MyConnection()))
                using (var destination = new SQLiteConnection($"Data Source={tempBackupPath};Version=3;"))
                {
                    source.Open();
                    destination.Open();
                    source.BackupDatabase(destination, "main", "main", -1, null, 0);
                }

                if (!ValidateSQLiteFile(tempBackupPath))
                {
                    try { File.Delete(tempBackupPath); } catch { }
                    return null;
                }

                return tempBackupPath;
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "CloudSync_CreateTempBackupSnapshot");
                try
                {
                    if (File.Exists(tempBackupPath))
                        File.Delete(tempBackupPath);
                }
                catch { }

                return null;
            }
        }

        private static bool ValidateSQLiteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    return false;

                FileInfo fi = new FileInfo(filePath);
                if (fi.Length <= 0)
                    return false;

                using (var test = new SQLiteConnection($"Data Source={filePath};Version=3;Busy Timeout=3000;"))
                {
                    test.Open();
                    using (var cmd = new SQLiteCommand("PRAGMA integrity_check;", test))
                    {
                        object result = cmd.ExecuteScalar();
                        return result != null &&
                               result.ToString().Equals("ok", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "CloudSync_ValidateSQLiteFile");
                return false;
            }
        }
    }
}