using EaseClub.Application.Common.Dtos;
using EaseClub.Domain.Common.Results;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Interfaces
{

    public interface IFileStorageService
    {
        
      public  Task<Result<FileUploadResult>> UploadFileAsync(Stream fileStream, string fileName, bool isPrivateFile = false, string folder = "/", CancellationToken cancellationToken = default);

      public string GetSignedUrl(string filePath, int expireInSeconds = 300);

      public string GetFileUrl(string filePath);
    }
}
