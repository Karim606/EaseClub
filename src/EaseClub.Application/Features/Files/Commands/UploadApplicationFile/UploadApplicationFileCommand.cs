using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Files;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Files.Commands.UploadApplicationFile
{
    public record UploadApplicationFileCommand(
    Guid ApplicationId,
    IFormFile File
    ):IRequest<Result<SecureFileResponse>>;
}
