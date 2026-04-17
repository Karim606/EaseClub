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
    public class UpdateClubDetailsHandler(
    IClubRepository clubRepo,
    IFileRepository fileRepo,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateClubDetailsCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(UpdateClubDetailsCommand request, CancellationToken ct)
        {
            // 1. Fetch the Aggregate
            var club = await clubRepo.GetByIdAsync(request.ClubId, ct);
            if (club == null) return Error.NotFound("Club not found");

            // 2. Map DTOs to Domain Value Objects
            var phoneNumber = PhoneNumber.Create(request.Data.Phone);
            var email = Email.Create(request.Data.Email);

            if (phoneNumber.IsError) return phoneNumber.TopError;
            if (email.IsError) return email.TopError;

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
                if (logo == null) return Error.NotFound("Logo file not found");

                if(logo.OwnerType != FileOwnerType.Club || logo.OwnerId != club.Id) 
                    return Error.Unauthorized("File does not belong to this club");
                // Mark the temporary upload as a permanent asset
                logo.MarkAsPermanent();
            }

            if (request.Data.coverImageId.HasValue)
            {
                coverImage = await fileRepo.GetByIdAsync(request.Data.coverImageId.Value, ct);
                if (coverImage == null) return Error.NotFound("Cover image file not found");

                if (coverImage.OwnerType != FileOwnerType.Club || coverImage.OwnerId != club.Id)
                    return Error.Unauthorized("File does not belong to this club");
                // Mark the temporary upload as a permanent asset
                coverImage.MarkAsPermanent();
            }
            string? logoUrl = null;
            string? coverImageUrl = null;

            if(logo != null) 
            logoUrl = fileStorageService.GetFileUrl(logo.FilePath);

            if(coverImage != null)
                coverImageUrl = fileStorageService.GetFileUrl(coverImage.FilePath);
            // 4. Execute Domain Logic
            club.UpdateDetails(
                request.Data.about,
                contactInfo,
                schedules.Select(s => s.Value),
                amenities,
                logoUrl,
                coverImageUrl
                );

            // 5. Persist Changes
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
