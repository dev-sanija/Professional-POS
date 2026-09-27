using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoUpdaterDotNET;

namespace PosSystem
{
    static class Program
    {
        private const string UpdateXmlUrl = "https://raw.githubusercontent.com/sanija123t/XML-POS/refs/heads/main/update.xml";
        private const string GoogleFormUrl = "https://docs.google.com/forms/d/e/1FAIpQLSd9Lyud9Ky5lCOEEGQwvdyxQNtbHdAANRKMxVUPuadj_3-CCA/formResponse";
        private static readonly string SourceDbPath = Path.Combine(Application.StartupPath, "DATABASE", "PosDB.db");
        private static readonly string PendingRestorePath = Path.Combine(Application.StartupPath, "DATABASE", "PosDB.pendingrestore");
        private static readonly string PendingRestoreMarkerPath = Path.Combine(Application.StartupPath, "DATABASE", "PosDB.restore.pending");
        private static readonly string RestoreSuccessFlagPath = Path.Combine(Application.StartupPath, "DATABASE", "PosDB.restore.success");
        private static readonly string PreRestoreSnapshotPath = Path.Combine(Application.StartupPath, "DATABASE", "PosDB.pre_restore_snapshot");
        private static readonly object UpdateSync = new object();
        private static int _updateCheckStarted = 0;

        [STAThread]
        static void Main()
        {
            Application.ThreadException += GlobalExceptionHandler;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ApplicationExit += Application_ApplicationExit;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                ApplyPendingRestoreIfNeeded();

                bool startupBackupOk = BackupDatabase();
                if (!startupBackupOk)
                {
                    MessageBox.Show(
                        "Warning: The startup backup could not be created. The system will continue, but update safety may be reduced.",
                        "Backup Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                // Core schema ownership stays in DBConnection.cs
                DBConnection.InitializeDatabase();

                // Only non-conflicting support tables here
                UpdateDatabaseSchema();
                ShowPendingRestoreSuccessIfNeeded();
                ConfigureAutoUpdater();

#if DEBUG
                Application.Run(new Form1());
#else
                Application.Run(new frmUserLogin());
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CRITICAL STARTUP ERROR: " + ex);
                SilentLog(ex, "System_Startup_Failure");

                MessageBox.Show(
                    "A startup error occurred. The details were logged for repair.",
                    "System Startup Note",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                Application.Exit();
            }
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            try
            {
                SafeReleaseDatabaseResources();
            }
            catch
            {
                // silent by design
            }
        }

        // ============================================================
        // UPDATER
        // ============================================================

        public static async Task<bool> HasWorkingInternetForUpdateAsync()
        {
            return await HasWorkingInternetAsync();
        }

        public static void BeginUiThreadUpdateCheck(Form owner)
        {
            try
            {
                if (owner == null || owner.IsDisposed)
                    return;

                if (Interlocked.Exchange(ref _updateCheckStarted, 1) == 1)
                    return;

                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 2000;

                EventHandler tickHandler = null;
                FormClosedEventHandler closeHandler = null;

                closeHandler = (s, e) =>
                {
                    try
                    {
                        timer.Stop();
                        timer.Dispose();
                    }
                    catch { }

                    Interlocked.Exchange(ref _updateCheckStarted, 0);

                    try
                    {
                        owner.FormClosed -= closeHandler;
                    }
                    catch { }
                };

                tickHandler = async (s, e) =>
                {
                    timer.Stop();
                    timer.Tick -= tickHandler;

                    try
                    {
                        if (owner.IsDisposed)
                        {
                            Interlocked.Exchange(ref _updateCheckStarted, 0);
                            return;
                        }

                        bool hasInternet = await HasWorkingInternetAsync();
                        if (!hasInternet)
                        {
                            Interlocked.Exchange(ref _updateCheckStarted, 0);
                            return;
                        }

                        AutoUpdater.Start(UpdateXmlUrl);
                    }
                    catch (Exception ex)
                    {
                        SilentLog(ex, "AutoUpdater_Start_Failure");
                        Interlocked.Exchange(ref _updateCheckStarted, 0);
                    }
                    finally
                    {
                        try
                        {
                            timer.Dispose();
                        }
                        catch { }

                        try
                        {
                            if (!owner.IsDisposed)
                                owner.FormClosed -= closeHandler;
                        }
                        catch { }
                    }
                };

                owner.FormClosed += closeHandler;
                timer.Tick += tickHandler;
                timer.Start();
            }
            catch (Exception ex)
            {
                SilentLog(ex, "BeginUiThreadUpdateCheck");
                Interlocked.Exchange(ref _updateCheckStarted, 0);
            }
        }

        private static void ConfigureAutoUpdater()
        {
            AutoUpdater.ReportErrors = false;
            AutoUpdater.Mandatory = false;
            AutoUpdater.UpdateMode = Mode.ForcedDownload;
            AutoUpdater.ShowSkipButton = true;
            AutoUpdater.ShowRemindLaterButton = true;

            // This event ONLY fires when the download is complete and it's time to swap the files
            AutoUpdater.ApplicationExitEvent += () =>
            {
                try
                {
                    // 1. Force a final backup before the app dies
                    bool updateBackupOk = BackupDatabase();
                    if (!updateBackupOk)
                    {
                        MessageBox.Show("Final safety backup failed! Proceeding at risk.", "Update Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    // 2. Safely release database locks and Clear SQLite pools
                    // This is critical to release file handles on the .db file
                    SafeReleaseDatabaseResources();

                    // 3. HARD KILL the process
                    // For Costura.Fody single-file apps, we must kill the process tree immediately
                    // so that ZipExtractor.exe can overwrite the locked PosSystem.exe file.
                    Process.GetCurrentProcess().Kill();
                }
                catch
                {
                    // Ensure the process is dead so the updater can work
                    Environment.Exit(0);
                }
            };
        }


        private static void SafeReleaseDatabaseResources()
        {
            try
            {
                // Pass 'true' to indicate the application is shutting down
                DBConnection.CloseAll(true);
            }
            catch (Exception ex)
            {
                SilentLog(ex, "SafeReleaseDatabaseResources_CloseAll");
            }

            try
            {
                SQLiteConnection.ClearAllPools();
            }
            catch (Exception ex)
            {
                SilentLog(ex, "SafeReleaseDatabaseResources_ClearPools");
            }
        }

        // ============================================================
        // BACKUP
        // ============================================================

        private static void DeleteBackupSidecarFiles(string basePath)
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
                SilentLog(ex, "Program_DeleteBackupSidecarFiles");
            }
        }

        private static bool BackupDatabase()
        {
            const int maxRetries = 3;
            const int delayBetweenRetriesMs = 1000;

            try
            {
                string dbFolder = Path.Combine(Application.StartupPath, "DATABASE");
                string dbFile = Path.Combine(dbFolder, "PosDB.db");
                string backupDir = Path.Combine(Application.StartupPath, "Backups");

                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                PurgeOldBackups(backupDir, 7);

                if (!File.Exists(dbFile))
                    return true;

                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string backupFile = Path.Combine(backupDir, $"PosDB_Backup_{timestamp}.db");

                        string sourceConString = $"Data Source={dbFile};Version=3;Busy Timeout=5000;";
                        string destinationConString = $"Data Source={backupFile};Version=3;Busy Timeout=5000;";

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

                        DeleteBackupSidecarFiles(backupFile);

                        if (ValidateBackupFile(backupFile))
                            return true;
                    }
                    catch (IOException) when (i < maxRetries - 1)
                    {
                        Thread.Sleep(delayBetweenRetriesMs);
                    }
                    catch (SQLiteException) when (i < maxRetries - 1)
                    {
                        Thread.Sleep(delayBetweenRetriesMs);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                SilentLog(ex, "Startup_Backup_Failure");
                return false;
            }
        }

        private static void PurgeOldBackups(string backupDir, int maxAgeDays)
        {
            try
            {
                DirectoryInfo dInfo = new DirectoryInfo(backupDir);
                foreach (FileInfo file in dInfo.GetFiles("PosDB_Backup_*.db"))
                {
                    try
                    {
                        if (file.CreationTime < DateTime.Now.AddDays(-maxAgeDays))
                            file.Delete();
                    }
                    catch
                    {
                        // ignore locked/used old backups
                    }
                }
            }
            catch (Exception ex)
            {
                SilentLog(ex, "PurgeOldBackups_Failure");
            }
        }

        private static bool ValidateBackupFile(string backupFile)
        {
            try
            {
                FileInfo backupInfo = new FileInfo(backupFile);
                if (!backupInfo.Exists || backupInfo.Length <= 0)
                    return false;

                using (SQLiteConnection test = new SQLiteConnection($"Data Source={backupFile};Version=3;Busy Timeout=3000;"))
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
            catch
            {
                return false;
            }
        }

        private static void ApplyPendingRestoreIfNeeded()
        {
            try
            {
                if (!File.Exists(PendingRestoreMarkerPath) || !File.Exists(PendingRestorePath))
                    return;

                if (!ValidatePendingRestoreFile(PendingRestorePath))
                {
                    try { File.Delete(PendingRestoreMarkerPath); } catch { }
                    try { if (File.Exists(PendingRestorePath)) File.Delete(PendingRestorePath); } catch { }
                    return;
                }

                try
                {
                    DBConnection.CloseAll();
                }
                catch (Exception ex)
                {
                    SilentLog(ex, "Program_ApplyPendingRestore_CloseAll");
                }

                try
                {
                    SQLiteConnection.ClearAllPools();
                }
                catch (Exception ex)
                {
                    SilentLog(ex, "Program_ApplyPendingRestore_ClearPools");
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                TryCreatePreRestoreSnapshot();
                DeleteRestoreSidecars();

                if (File.Exists(SourceDbPath))
                    File.Replace(PendingRestorePath, SourceDbPath, null);
                else
                    File.Move(PendingRestorePath, SourceDbPath);

                DeleteRestoreSidecars();

                try { File.Delete(PendingRestoreMarkerPath); } catch { }
                try { File.WriteAllText(RestoreSuccessFlagPath, DateTime.Now.ToString("O")); } catch { }
            }
            catch (Exception ex)
            {
                SilentLog(ex, "Program_ApplyPendingRestoreIfNeeded");

                try { if (File.Exists(PendingRestoreMarkerPath)) File.Delete(PendingRestoreMarkerPath); } catch { }
                try { if (File.Exists(PendingRestorePath)) File.Delete(PendingRestorePath); } catch { }
            }
        }

        private static bool ValidatePendingRestoreFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    return false;

                FileInfo fi = new FileInfo(filePath);
                if (fi.Length <= 0)
                    return false;

                using (SQLiteConnection test = new SQLiteConnection($"Data Source={filePath};Version=3;Busy Timeout=3000;"))
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
            catch
            {
                return false;
            }
        }

        private static void TryCreatePreRestoreSnapshot()
        {
            try
            {
                if (!File.Exists(SourceDbPath))
                    return;

                using (SQLiteConnection source = new SQLiteConnection($"Data Source={SourceDbPath};Version=3;Busy Timeout=5000;"))
                using (SQLiteConnection destination = new SQLiteConnection($"Data Source={PreRestoreSnapshotPath};Version=3;"))
                {
                    source.Open();
                    destination.Open();
                    source.BackupDatabase(destination, "main", "main", -1, null, 0);
                }
            }
            catch (Exception ex)
            {
                SilentLog(ex, "Program_TryCreatePreRestoreSnapshot");
            }
        }

        private static void DeleteRestoreSidecars()
        {
            try
            {
                if (File.Exists(SourceDbPath + "-wal")) File.Delete(SourceDbPath + "-wal");
                if (File.Exists(SourceDbPath + "-shm")) File.Delete(SourceDbPath + "-shm");
            }
            catch (Exception ex)
            {
                SilentLog(ex, "Program_DeleteRestoreSidecars");
            }
        }

        private static void ShowPendingRestoreSuccessIfNeeded()
        {
            try
            {
                if (!File.Exists(RestoreSuccessFlagPath))
                    return;

                try { File.Delete(RestoreSuccessFlagPath); } catch { }

                MessageBox.Show(
                    "Successfully restored database backup.",
                    "Restore Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SilentLog(ex, "Program_ShowPendingRestoreSuccessIfNeeded");
            }
        }

        // ============================================================
        // ERROR HANDLING / CLOUD LOGGING
        // ============================================================

        private static void GlobalExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            HandleFatalError(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleFatalError(e.ExceptionObject as Exception ?? new Exception("Unknown fatal error."));
        }

        private static void HandleFatalError(Exception ex)
        {
            try
            {
                string logPath = Path.Combine(Application.StartupPath, "crash_log.txt");
                File.AppendAllText(
                    logPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} :: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{Environment.NewLine}");
            }
            catch
            {
                // silent
            }

            string hwid = GetHardwareID();
            string pcUser = Environment.MachineName + "_" + Environment.UserName;
            string location = ex.TargetSite?.DeclaringType?.Name ?? "Unknown";
            string details = "Msg: " + ex.Message + Environment.NewLine + Environment.NewLine + "Stack: " + ex.StackTrace;

            Task.Run(async () =>
            {
                try
                {
                    await SendErrorToCloud(hwid, pcUser, location, details);
                }
                catch
                {
                    // silent
                }
            });

            MessageBox.Show(
                "A system error was detected:" + Environment.NewLine + Environment.NewLine + ex.Message,
                "POS System Note",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        public static void SilentLog(Exception ex, string formName)
        {
            if (ex == null)
                return;

            string hwid = GetHardwareID();
            string pcUser = Environment.MachineName + "_" + Environment.UserName;
            string details = "SILENT_DB_ERROR: " + ex.Message + Environment.NewLine + ex.StackTrace;

            Task.Run(async () =>
            {
                try
                {
                    await SendErrorToCloud(hwid, pcUser, formName, details);
                }
                catch
                {
                    // silent
                }
            });
        }

        private static async Task SendErrorToCloud(string hwid, string pcUser, string location, string details)
        {
            try
            {
                bool hasInternet = await HasWorkingInternetAsync();
                if (!hasInternet)
                    return;

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(6);

                    var formData = new Dictionary<string, string>
                    {
                        { "entry.1338834817", hwid ?? "UNKNOWN_ID" },
                        { "entry.1074494650", pcUser ?? "UNKNOWN_USER" },
                        { "entry.671496035", location ?? "UNKNOWN_LOCATION" },
                        { "entry.2072454999", details ?? "NO_DETAILS" }
                    };

                    using (FormUrlEncodedContent content = new FormUrlEncodedContent(formData))
                    {
                        await client.PostAsync(GoogleFormUrl, content);
                    }
                }
            }
            catch
            {
                // Stay silent by design.
            }
        }

        private static async Task<bool> HasWorkingInternetAsync()
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

        private static string GetHardwareID()
        {
            try
            {
                string id = string.Empty;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        try
                        {
                            if (obj["ProcessorId"] != null)
                                id = obj["ProcessorId"].ToString();
                        }
                        finally
                        {
                            obj.Dispose();
                        }
                    }
                }

                return string.IsNullOrWhiteSpace(id) ? "UNKNOWN_ID" : id;
            }
            catch
            {
                return "UNKNOWN_ID";
            }
        }

        // ============================================================
        // LEGACY SUPPORT TABLES ONLY
        // Core schema/migrations belong to DBConnection.cs
        // ============================================================

        private static void UpdateDatabaseSchema()
        {
            using (SQLiteConnection con = new SQLiteConnection(DBConnection.MyConnection()))
            {
                con.Open();

                using (SQLiteTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        string[] supportTables =
                        {
                            "CREATE TABLE IF NOT EXISTS tblAdjustment (id INTEGER PRIMARY KEY AUTOINCREMENT, referenceno TEXT, pcode TEXT, qty INTEGER, action TEXT, remarks TEXT, sdate TEXT, [user] TEXT)",
                            "CREATE TABLE IF NOT EXISTS tblAudit (id INTEGER PRIMARY KEY AUTOINCREMENT, [user] TEXT, date TEXT, action TEXT)"
                        };

                        foreach (string sql in supportTables)
                        {
                            using (SQLiteCommand cmd = new SQLiteCommand(sql, con, transaction))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { }
                        SilentLog(ex, "UpdateDatabaseSchema_Failure");
                        throw;
                    }
                }
            }
        }
    }
}