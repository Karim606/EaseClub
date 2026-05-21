using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Files;
using EaseClub.Domain.MembershipApplications;
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
    ) : IRequest<Result<SecureFileResponse>>, IRequireResourceValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                (auth, _) => auth.DoesResourceBelongToCurrentUserAsync<MembershipApplication>(ApplicationId),
                nameof(MembershipApplication),
                ApplicationId
                );
        }
    }
}
