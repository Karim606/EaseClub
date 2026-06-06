using Microsoft.Extensions.Logging;
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
        ICurrentUserService currentUserService,
        ILogger<UploadClubFileCommandHandler> logger) : IRequestHandler<UploadClubFileCommand, Result<SecureFileResponse>>
    {


        public async Task<Result<SecureFileResponse>> Handle(UploadClubFileCommand request, CancellationToken ct)
        {
            if (request.File == null || request.File.Length == 0) { logger.LogError("Validation error in UploadClubFileCommandHandler: {Error}", Error.Validation("File is empty").ToLogObject()); return Error.Validation("File is empty"); }

            var parsingRes = Guid.TryParse(currentUserService.GetId(), out var userId);
            if (!parsingRes) { logger.LogError("Unauthorized error in UploadClubFileCommandHandler: {Error}", Error.Unauthorized().ToLogObject()); return Error.Unauthorized(); }

            var folder = $"clubs/{request.ClubId}/";

            if(request.Purpose == FilePurpose.EventImage)
                folder += $"Events/";

            var res = await fileStorageService.UploadFileAsync(request.File.OpenReadStream(), request.File.FileName,false,folder, cancellationToken: ct);

            if (res.IsError) { logger.LogError("Error in UploadClubFileCommandHandler: {Error}", res.TopError.ToLogObject()); return res.TopError; }
            var file = FileResource.Create(Guid.NewGuid(), res.Value.FileName, res.Value.FilePath, request.File.ContentType, res.Value.Size, userId,false, request.ClubId,request.Purpose, FileOwnerType.Club);

            if (file.IsError) { logger.LogError("Error in UploadClubFileCommandHandler: {Error}", file.TopError.ToLogObject()); return file.TopError; }
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
