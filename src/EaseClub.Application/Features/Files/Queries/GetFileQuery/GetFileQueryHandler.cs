using EaseClub.Application.Common.Interfaces;
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
        ICurrentUserService currentUserService) : IRequestHandler<GetFileQuery, Result<FileDto>>
    {
        

        public async Task<Result<FileDto>> Handle(GetFileQuery request, CancellationToken ct)
        {
            var parsingRes = Guid.TryParse(currentUserService.GetId(),out var userId);

            if (parsingRes == false)
                return Error.Unauthorized();

            var roles = currentUserService.GetRoles();
            var file = await fileRepository.GetByIdAsync(request.FileId, ct);

            if (file == null)
                return Error.NotFound("File not found");

            var isSuperAdmin = roles.Contains("SuperAdmin");

            if (!isSuperAdmin && roles.Contains("ClubAdmin"))
            {
                var admin = await clubAdminUserRepository.GetByIdAsync(userId, ct);
                if (admin == null) return Error.Unauthorized();

                if (admin.ClubId != file.ClubId)
                    return Error.Unauthorized();
            }

            else if (!isSuperAdmin && file.CreatedBy != userId)
                return Error.Unauthorized();

            string? url;
            if(file.IsPrivate)
             url = fileStorageService.GetSignedUrl("s");
            else
                url = fileStorageService.GetFileUrl(file.FilePath);

                var dto = new FileDto(
                    file.Id,
                    file.FileName,
                    url,
                    file.ContentType,
                    file.Size,
                    file.Category
                );

            return dto;
        }
    }
}
