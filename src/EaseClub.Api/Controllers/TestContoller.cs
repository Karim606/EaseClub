using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        // GET: /api/test
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Message = "EaseClub API is working!",
                Time = DateTime.UtcNow
            });
        }
    }
}
