using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Template.UpsertTemplate
{
    public record UpsertTemplateCommand(
    Guid ClubId,
    Guid? TemplateId, // Null for Create
    string Name,
    List<StepDetailsDto> Steps) : IRequest<Result<Success>>, IRequireClubAdmin;
}
