using EaseClub.Domain.Common.Results;
using MediatR;

namespace EaseClub.Application.Features.InstallmentTemplates.Commands.ToggleInstallmentTemplateStatus
{
    public record ToggleInstallmentTemplateStatusCommand(Guid Id) : IRequest<Result<Success>>;
}
