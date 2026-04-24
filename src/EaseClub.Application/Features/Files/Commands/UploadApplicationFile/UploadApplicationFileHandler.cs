using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Files;
using EaseClub.Domain.Files.Enums;
using EaseClub.Domain.Member;
using EaseClub.Domain.MembershipApplications.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EaseClub.Application.Features.Files.Commands.UploadApplicationFile
{
    public class UploadApplicationFileHandler(IFileStorageService fileStorageService,
        IFileRepository fileRepository,
        IMembershipApplicationRepository membershipApplicationRepository,
        IMemberUserRepository memberUserRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UploadApplicationFileHandler> logger) :IRequestHandler<UploadApplicationFileCommand, Result<SecureFileResponse>>
    {


        public async Task<Result<SecureFileResponse>> Handle(UploadApplicationFileCommand request, CancellationToken ct)
        {
            if (request.File == null || request.File.Length == 0) { logger.LogError("Validation error in UploadApplicationFileHandler: {Error}", Error.Validation("File is empty").ToLogObject()); return Error.Validation("File is empty"); }
            var app = await membershipApplicationRepository.GetByIdAsync(request.ApplicationId, ct);
            if (app == null) { logger.LogError("NotFound error in UploadApplicationFileHandler: {Error}", Error.NotFound("Application not found").ToLogObject()); return Error.NotFound("Application not found"); }
            var roles = currentUserService.GetRoles();

            var parsingRes = Guid.TryParse(currentUserService.GetId(), out var userId);
            if (!parsingRes) { logger.LogError("Unauthorized error in UploadApplicationFileHandler: {Error}", Error.Unauthorized().ToLogObject()); return Error.Unauthorized(); }
            if (roles.Contains("Member"))
            {
                if (app.MemberId != userId) { logger.LogError("Unauthorized error in UploadApplicationFileHandler: {Error}", Error.Unauthorized().ToLogObject()); return Error.Unauthorized(); }
            }
            else if(!roles.Contains("SuperAdmin")) 
                return Error.Unauthorized();


            var res = await fileStorageService.UploadFileAsync(request.File.OpenReadStream(), request.File.Name,true, $"clubs/{app.ClubId}/applications/{app.Id}/", cancellationToken: ct);

            if (res.IsError) { logger.LogError("Error in UploadApplicationFileHandler: {Error}", res.TopError.ToLogObject()); return res.TopError; }
            var file = FileResource.Create(Guid.NewGuid(),res.Value.FileName,res.Value.FilePath,request.File.ContentType,res.Value.Size,userId,true,app.Id,FilePurpose.ApplicationDocument,FileOwnerType.Application);

            if (file.IsError) { logger.LogError("Error in UploadApplicationFileHandler: {Error}", file.TopError.ToLogObject()); return file.TopError; }
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
