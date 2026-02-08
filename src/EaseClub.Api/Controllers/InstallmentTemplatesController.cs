using EaseClub.Application.Features.InstallmentTemplates.Commands.CreateInstallmentTemplate;
using EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplateById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/{version:ApiVersion}/clubs/{clubId}/installment-templates")]
    public class InstallmentTemplates(ISender sender) : ApiController
    {

        [Authorize(Roles = "ClubAdmin")]
        [HttpPost]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateTemplate(Guid clubId, CreateInstallmentTemplateCommand request)
        {
            var command = new CreateInstallmentTemplateCommand(
                request.Name,
                request.NumOfInstallments,
                request.DurationInDays,
                request.Installments)
                { ClubId = clubId };

            var result = await sender.Send(command);
            return result.Match(
                id => CreatedAtAction(nameof(GetTemplate), new { version="1.0",clubId, id }, id),
                Problem);
        }

        [HttpGet("{id:guid}")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetTemplate(Guid id)
        {

            var query = new GetInstallmentTemplateQuery(id);
            var result = await sender.Send(query);

            return result.Match(
                  (template) => Ok(template),
                  Problem);
        }
    }
}
