using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// Posts JSON to the Apps Script web app. Apps Script answers a POST with a redirect to the result,
    /// which HttpClient follows as a GET.
    /// </summary>
    public static class SheetWebClient
    {
        private const string JsonMediaType = "application/json";
        private const int TimeoutSeconds = 90;
        private const int ErrorSnippetLength = 200;
        private const char JsonObjectStart = '{';

        private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(TimeoutSeconds) };

        /// <summary>Sends a request object and returns the parsed response. Throws on network or protocol errors.</summary>
        public static async Task<SheetResponse> PostAsync(string url, object request)
        {
            string body = JsonUtility.ToJson(request);
            using (var content = new StringContent(body, Encoding.UTF8, JsonMediaType))
            using (HttpResponseMessage response = await Client.PostAsync(url, content))
            {
                string text = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        $"Sheet web app returned HTTP {(int)response.StatusCode}: {Snippet(text)}");
                }

                string trimmed = text.TrimStart();
                if (trimmed.Length == 0 || trimmed[0] != JsonObjectStart)
                {
                    throw new InvalidOperationException(
                        "Sheet web app did not return JSON. Check the deployment URL and that the web app " +
                        "is deployed with access set to Anyone. Response: " + Snippet(text));
                }

                SheetResponse parsed = JsonUtility.FromJson<SheetResponse>(text);
                if (parsed == null || !parsed.ok)
                {
                    throw new InvalidOperationException("Sheet web app error: " +
                                                        (parsed == null ? Snippet(text) : parsed.error));
                }

                return parsed;
            }
        }

        private static string Snippet(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "(empty)";
            }

            return text.Length <= ErrorSnippetLength ? text : text.Substring(0, ErrorSnippetLength) + "...";
        }
    }
}
