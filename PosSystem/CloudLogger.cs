using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public static class CloudLogger
{
    // Verified Google Form endpoint
    private static readonly string formUrl =
        "https://docs.google.com/forms/d/e/1FAIpQLSd9Lyud9Ky5lCOEEGQwvdyxQNtbHdAANRKMxVUPuadj_3-CCA/formResponse";

    public static async Task SendError(string hwid, string pcUser, string location, string details)
    {
        try
        {
            bool hasInternet = await HasWorkingInternetAsync();
            if (!hasInternet) return;

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(6);

                var formData = new Dictionary<string, string>
                {
                    { "entry.1338834817", string.IsNullOrWhiteSpace(hwid) ? "UNKNOWN_ID" : hwid },
                    { "entry.1074494650", string.IsNullOrWhiteSpace(pcUser) ? "UNKNOWN_USER" : pcUser },
                    { "entry.671496035", string.IsNullOrWhiteSpace(location) ? "UNKNOWN_LOCATION" : location },
                    { "entry.2072454999", string.IsNullOrWhiteSpace(details) ? "NO_DETAILS" : details }
                };

                using (var content = new FormUrlEncodedContent(formData))
                using (var response = await client.PostAsync(formUrl, content))
                {
                    // Intentionally no popup / no throw.
                    // We stay silent even if the server rejects it.
                }
            }
        }
        catch
        {
            // Stay silent - no popups if upload fails
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
}