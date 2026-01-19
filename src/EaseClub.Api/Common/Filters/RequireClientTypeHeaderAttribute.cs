using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EaseClub.Api.Common.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequireClientTypeHeaderAttribute:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var headers = context.HttpContext.Request.Headers;
            if (!headers.TryGetValue("X-Client-Type", out var clientType) ||
                (clientType != "Web" && clientType != "Mobile"))
            {
                context.Result = new BadRequestObjectResult(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Missing or invalid 'X-Client-Type' header. Must be 'Web' or 'Mobile'."
                });
                return;
            }

            // Store it in HttpContext.Items for later access in the controller or services
            context.HttpContext.Items["ClientType"] = clientType.ToString();

            base.OnActionExecuting(context);
        }
    }
}
