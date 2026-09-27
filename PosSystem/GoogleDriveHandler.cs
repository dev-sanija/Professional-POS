using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosSystem
{
    public class GoogleDriveHandler
    {
        private static string[] Scopes = { DriveService.Scope.DriveFile };
        private const string ApplicationName = "PosSystemCloud";

        public async Task<DriveService> GetServiceAsync()
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PosSystem");
            string credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");

            try
            {
                if (!Directory.Exists(appDataFolder)) Directory.CreateDirectory(appDataFolder);

                UserCredential credential;
                using (var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read))
                {
                    // 🎯 DIAGNOSTIC: We set a timeout. If it doesn't open in 15s, it will throw an error.
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(TimeSpan.FromSeconds(15));

                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        cts.Token,
                        new FileDataStore(appDataFolder, true)
                    );
                }

                return new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
            catch (OperationCanceledException)
            {
                // This confirms the library is HANGING and we timed it out.
                MessageBox.Show("The Auth process timed out. A firewall or Antivirus might be blocking the browser launch.", "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"DETAILED ERROR:\nType: {ex.GetType().Name}\nMessage: {ex.Message}\n\nCheck if Newtonsoft.Json is installed!", "Debug Info");
                throw;
            }
        }
    }
}