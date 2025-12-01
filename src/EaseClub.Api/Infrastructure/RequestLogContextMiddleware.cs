using Serilog.Context;


namespace EaseClub.Api.Infrastructure
{
    public class RequestLogContextMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = context.TraceIdentifier;

            using  (LogContext.PushProperty("RequestId", requestId))
            {
                
                await _next(context);
            }
        }
    }
}
