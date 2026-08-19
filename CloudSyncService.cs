using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MAMEIronXP
{
    public class CloudSyncService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly string _appSettingsPath;
        private readonly Logger _logger;

        public CloudSyncService(string appSettingsPath, Logger logger)
        {
            _appSettingsPath = appSettingsPath;
            _logger = logger;
        }

        public bool IsEnabled => GetCloudSyncSection(ReadRoot())["Enabled"]?.Value<bool>() ?? false;

        public void SetEnabled(bool enabled)
        {
            JObject root = ReadRoot();
            GetCloudSyncSection(root)["Enabled"] = enabled;
            WriteRoot(root);
        }

        /// <summary>
        /// PATCHes user-data.json to the cloud, if CloudSync is enabled and an Endpoint is configured.
        /// Generates and persists the CloudSync:UserId GUID the first time it's needed.
        /// </summary>
        public async Task PersistUserDataAsync(string userDataJsonPath)
        {
            JObject root = ReadRoot();
            JObject cloudSync = GetCloudSyncSection(root);

            bool enabled = cloudSync["Enabled"]?.Value<bool>() ?? false;
            if (!enabled)
            {
                return;
            }

            string endpoint = cloudSync["Endpoint"]?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                _logger.LogInfo("CloudSync: enabled but CloudSync:Endpoint is not configured in appsettings.json; skipping upload.");
                return;
            }

            string? userId = cloudSync["UserId"]?.ToString();
            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = Guid.NewGuid().ToString();
                cloudSync["UserId"] = userId;
                WriteRoot(root);
            }

            if (!File.Exists(userDataJsonPath))
            {
                _logger.LogInfo($"CloudSync: skipped upload because {userDataJsonPath} does not exist.");
                return;
            }

            string json = await File.ReadAllTextAsync(userDataJsonPath);
            string url = $"{endpoint.TrimEnd('/')}/user-data/{userId}";

            HttpResponseMessage response;
            try
            {
                using StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                response = await _httpClient.PatchAsync(url, content);
            }
            catch (Exception)
            {
                // Couldn't reach the server at all (no internet, DNS failure, timeout, etc.) - fail silently.
                return;
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInfo($"CloudSync: PATCHed user data to the cloud for user {userId}.");
            }
            else
            {
                string body = await response.Content.ReadAsStringAsync();
                _logger.LogInfo($"CloudSync: API returned {(int)response.StatusCode} {response.StatusCode} for user {userId}: {body}");
            }
        }

        private JObject ReadRoot()
        {
            return File.Exists(_appSettingsPath)
                ? JObject.Parse(File.ReadAllText(_appSettingsPath))
                : new JObject();
        }

        private void WriteRoot(JObject root)
        {
            File.WriteAllText(_appSettingsPath, root.ToString(Formatting.Indented));
        }

        private static JObject GetCloudSyncSection(JObject root)
        {
            if (root["CloudSync"] is not JObject cloudSync)
            {
                cloudSync = new JObject();
                root["CloudSync"] = cloudSync;
            }
            return cloudSync;
        }
    }
}
