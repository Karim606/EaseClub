using EaseClub.Application.Common.Dtos;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace EaseClub.Infrastructure.Services
{

    public class ImageKitFileStorageService : IFileStorageService
    {
        private readonly string _baseUrl;
        private readonly string _privateApiKey;
        private readonly string _publicApiKey;
        private readonly HttpClient _httpClient = new HttpClient();
        public ImageKitFileStorageService(IConfiguration config)
        {

            _publicApiKey = config["ImageKit:PublicKey"];
            _privateApiKey = config["ImageKit:PrivateKey"];
;             _baseUrl = config["ImageKit:UrlEndpoint"];
        }

        public async Task<Result<FileUploadResult>> UploadFileAsync(Stream fileStream, string fileName, bool isPrivateFile = false, string folder = "/",CancellationToken cancellationToken = default)
        {
            using var content = new MultipartFormDataContent();
            var uploadUrl = "https://upload.imagekit.io/api/v1/files/upload";
            // File content
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"file\"",
                FileName = $"\"{fileName}\""
            };
            content.Add(fileContent);

            // Other form data
            content.Add(new StringContent(fileName), "fileName");
            content.Add(new StringContent(isPrivateFile.ToString().ToLower()), "isPrivateFile");
            content.Add(new StringContent(folder), "folder");

            // Authentication header (Basic Auth)
            var authHeader = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_privateApiKey}:"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var response = await _httpClient.PostAsync(uploadUrl, content);
            response.EnsureSuccessStatusCode();

            if (response.StatusCode != System.Net.HttpStatusCode.OK) return Error.Failure(description:"Failed to upload file.");
            var json = await response.Content.ReadAsStringAsync();

            // Deserialize only the fields we care about
            var result = JsonSerializer.Deserialize<FileUploadResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }


        public string GetSignedUrl(string filePath, int expireInSeconds = 300)
        {
            // Normalize base URL
            var endpoint = _baseUrl.TrimEnd('/');

            // Normalize path
            filePath = filePath.TrimStart('/');

            // Expiry
            long expireTime =
                DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expireInSeconds;

            // EXACT string ImageKit signs
            string dataToSign = filePath + expireTime;

            using var hmac = new HMACSHA1(
                Encoding.UTF8.GetBytes(_privateApiKey)
            );

            byte[] hash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(dataToSign)
            );

            string signature = BitConverter
                .ToString(hash)
                .Replace("-", "")
                .ToLowerInvariant();

            return $"{endpoint}/{filePath}?ik-t={expireTime}&ik-s={signature}";
        }

        public string GetFileUrl(string filePath)
        {
            return $"{_baseUrl}{"/"+filePath}";
        }
    }
}
