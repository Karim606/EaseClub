using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController(ISender sender) : ControllerBase
    {
    }
}
