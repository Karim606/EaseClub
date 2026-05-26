using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Clubs.ValueObjects;
using EaseClub.Domain.Common;
using EaseClub.Domain.Files.Enums;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Common.ValueObjects;
using EaseClub.Domain.Files;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Clubs.Commands.UpdateClubDetails
{
    public class UpdateClubDetailsHandler(IClubRepository clubRepo,
    IFileRepository fileRepo,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork,
        ILogger<UpdateClubDetailsHandler> logger) : IRequestHandler<UpdateClubDetailsCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(UpdateClubDetailsCommand request, CancellationToken ct)
        {
            // 1. Fetch the Aggregate
            var club = await clubRepo.GetByIdAsync(request.ClubId, ct);
            if (club == null) { logger.LogError("NotFound error in UpdateClubDetailsHandler: {Error}", Error.NotFound("Club not found").ToLogObject()); return Error.NotFound("Club not found"); }

            // 2. Map DTOs to Domain Value Objects
            var phoneNumber = PhoneNumber.Create(request.Data.Phone);
            var email = Email.Create(request.Data.Email);

            if (phoneNumber.IsError) { logger.LogError("Error in UpdateClubDetailsHandler: {Error}", phoneNumber.TopError.ToLogObject()); return phoneNumber.TopError; }
            if (email.IsError) { logger.LogError("Error in UpdateClubDetailsHandler: {Error}", email.TopError.ToLogObject()); return email.TopError; }
            var contactInfo = new ContactInfo(phoneNumber.Value, email.Value);

            var schedules = request.Data.WorkSchedules
                .Select(s => WorkSchedule.Create(s.Label, s.TimeRange))
                .ToList();

            // Check if any schedule creation failed validation
            if (schedules.Any(s => s.IsError))
                return schedules.First(s => s.IsError).TopError;

            var amenities = request.Data.Amenities.Select(a => new Amenity(a));

            // 3. Handle File Lifecycle (The Logo)
            FileResource? logo = null;
            FileResource? coverImage = null;
            if (request.Data.LogoId.HasValue)
            {
                 logo = await fileRepo.GetByIdAsync(request.Data.LogoId.Value, ct);
                if (logo == null) { logger.LogError("NotFound error in UpdateClubDetailsHandler: {Error}", Error.NotFound("Logo file not found").ToLogObject()); return Error.NotFound("Logo file not found"); }
            if (logo.OwnerType != FileOwnerType.Club || logo.OwnerId != club.Id) { logger.LogError("Unauthorized error in UpdateClubDetailsHandler: {Error}", Error.Unauthorized("File does not belong to this club").ToLogObject()); return Error.Unauthorized("File does not belong to this club"); }
                // Mark the temporary upload as a permanent asset
                logo.MarkAsPermanent();
            }
            if (request.Data.coverImageId.HasValue)
            {
                coverImage = await fileRepo.GetByIdAsync(request.Data.coverImageId.Value, ct);
                if (coverImage == null) { logger.LogError("NotFound error in UpdateClubDetailsHandler: {Error}", Error.NotFound("Cover image file not found").ToLogObject()); return Error.NotFound("Cover image file not found"); }
            if (coverImage.OwnerType != FileOwnerType.Club || coverImage.OwnerId != club.Id) { logger.LogError("Unauthorized error in UpdateClubDetailsHandler: {Error}", Error.Unauthorized("File does not belong to this club").ToLogObject()); return Error.Unauthorized("File does not belong to this club"); }
                // Mark the temporary upload as a permanent asset
                coverImage.MarkAsPermanent();
            }
            // 4. Capture current IDs for cleanup
            var oldLogoId = club.LogoId;
            var oldCoverId = club.CoverImageId;

            // 5. Execute Domain Logic
            club.UpdateDetails(
                request.Data.about,
                contactInfo,
                schedules.Select(s => s.Value),
                amenities,
                request.Data.LogoId,
                request.Data.coverImageId
                );

            // 6. Cleanup old FileResources from the database
            if (oldLogoId.HasValue && oldLogoId.Value != request.Data.LogoId)
            {
                var oldLogo = await fileRepo.GetByIdAsync(oldLogoId.Value, ct);
                if (oldLogo != null)
                {
                    await fileRepo.DeleteAsync(oldLogo, ct);
                }
            }

            if (oldCoverId.HasValue && oldCoverId.Value != request.Data.coverImageId)
            {
                var oldCover = await fileRepo.GetByIdAsync(oldCoverId.Value, ct);
                if (oldCover != null)
                {
                    await fileRepo.DeleteAsync(oldCover, ct);
                }
            }

            // 5. Persist Changes
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
