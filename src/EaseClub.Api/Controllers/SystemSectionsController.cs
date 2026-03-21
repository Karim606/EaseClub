using EaseClub.Application.Features.ApplicationTemplates.Queries.GetSystemSection;
using EaseClub.Domain.ApplicationTemplates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/system-sections")]
    public class SystemSectionsController(ISender sender) : ApiController
    {

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpGet("{intent}")]
        [EndpointSummary("Retrieves the system blueprint for a specific section intent")]
        [EndpointDescription("Returns the predefined fields and validation rules for system-managed sections like FamilyMembers. Use this to ensure custom templates align with system requirements.")]

        public async Task<IActionResult> GetSystemSection([FromRoute] SectionIntent intent)
        {
            var result = await sender.Send(new GetSystemSectionQuery(intent));
            
            return result.Match(
                (section) => Ok(section),
                Problem
                );
        }
    }
}
