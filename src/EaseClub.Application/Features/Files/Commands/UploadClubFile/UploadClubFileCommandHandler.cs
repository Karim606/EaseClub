using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Files;
using EaseClub.Domain.Files.Enums;
using EaseClub.Domain.Member;
using EaseClub.Domain.MembershipApplications.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Files.Commands.UploadClubFile
{
    public class UploadClubFileCommandHandler
        (IFileStorageService fileStorageService,
        IFileRepository fileRepository,
        IClubAdminUserRepository clubAdminUserRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<UploadClubFileCommand, Result<SecureFileResponse>>
    {


        public async Task<Result<SecureFileResponse>> Handle(UploadClubFileCommand request, CancellationToken ct)
        {
            if (request.File == null || request.File.Length == 0)
                return Error.Validation("File is empty");

            var roles = currentUserService.GetRoles();

            var parsingRes = Guid.TryParse(currentUserService.GetId(), out var userId);
            if (!parsingRes)
                return Error.Unauthorized();

            if (roles.Contains("ClubAdmin"))
            {
                var admin = await clubAdminUserRepository.GetByIdAsync(userId, ct);
                if(admin == null) return Error.Unauthorized();

                if (admin.ClubId != request.ClubId)
                    return Error.Forbidden();

            }

            else if (!roles.Contains("SuperAdmin"))
                return Error.Unauthorized();


            var res = await fileStorageService.UploadFileAsync(request.File.OpenReadStream(), request.File.Name, true, $"clubs/{request.ClubId}/", cancellationToken: ct);

            if (res.IsError) return res.TopError;

            var file = FileResource.Create(Guid.NewGuid(), res.Value.FileName, res.Value.FilePath, request.File.ContentType, res.Value.Size, userId, true, request.ClubId,request.Purpose, FileOwnerType.Club);

            if (file.IsError) return file.TopError;

            await fileRepository.AddAsync(file.Value, ct);
            await unitOfWork.SaveChangesAsync(ct);

            var response = new SecureFileResponse(
                file.Value.Id,
                request.File.FileName
            );

            return response;
        }
    }
}
