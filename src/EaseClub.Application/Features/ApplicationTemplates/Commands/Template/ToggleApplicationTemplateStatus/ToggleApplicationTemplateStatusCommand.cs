using EaseClub.Domain.Common.Results;
using MediatR;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Template.ToggleApplicationTemplateStatus
{
    public record ToggleApplicationTemplateStatusCommand(Guid Id) : IRequest<Result<Success>>;
}
