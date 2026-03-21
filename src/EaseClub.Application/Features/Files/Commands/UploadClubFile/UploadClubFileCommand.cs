using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Files.Commands.UploadClubFile
{
    public record UploadClubFileCommand(
     IFormFile File,
     Guid ClubId,
     bool IsPrivate = false
     ) : IRequest<Result<SecureFileResponse>>;
}
