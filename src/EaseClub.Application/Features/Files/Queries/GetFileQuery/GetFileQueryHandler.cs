using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Files.Commands;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Files;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Files.Queries.GetFileQuery
{
    public class GetFileQueryHandler(IFileRepository fileRepository,
        IClubAdminUserRepository clubAdminUserRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        FileAuthorizationService fileAuthorizationService) : IRequestHandler<GetFileQuery, Result<FileDto>>
    {
        

        public async Task<Result<FileDto>> Handle(GetFileQuery request, CancellationToken ct)
        {
            var file = await fileRepository.GetByIdAsync(request.FileId, ct);

            if (file == null)
                return Error.NotFound("File not found");

            var canAccess = await fileAuthorizationService.CanAccessAsync(file);

                if (!canAccess)
                    return Error.Unauthorized("You do not have permission to access this file");

            string? url;
            if(file.IsPrivate)
             url = fileStorageService.GetSignedUrl(file.FilePath,900);
            else
                url = fileStorageService.GetFileUrl(file.FilePath);

                var dto = new FileDto(
                    file.Id,
                    file.FileName,
                    url,
                    file.ContentType,
                    file.Size,
                    file.Purpose
                );

            return dto;
        }
    }
}
