using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CloudSyncApi
{
    public class UserDataFunctions
    {
        private const string ContainerName = "user-data";

        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<UserDataFunctions> _logger;

        public UserDataFunctions(BlobServiceClient blobServiceClient, ILogger<UserDataFunctions> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        /// <summary>
        /// Stores the caller's user-data.json contents as a blob named "{guid}.json", overwriting
        /// whatever was there before. The GUID is the cabinet's CloudSync:UserId from appsettings.json.
        /// </summary>
        [Function("PersistUserData")]
        public async Task<IActionResult> PersistUserData(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "user-data/{guid:guid}")] HttpRequest req,
            Guid guid)
        {
            string body;
            using (StreamReader reader = new StreamReader(req.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            if (!IsValidJson(body))
            {
                return new BadRequestObjectResult("Request body must be valid JSON.");
            }

            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            BlobClient blobClient = containerClient.GetBlobClient($"{guid}.json");

            using (MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(body)))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            _logger.LogInformation("Persisted user data for {UserId}", guid);
            return new OkResult();
        }

        private static bool IsValidJson(string text)
        {
            try
            {
                using JsonDocument _ = JsonDocument.Parse(text);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
