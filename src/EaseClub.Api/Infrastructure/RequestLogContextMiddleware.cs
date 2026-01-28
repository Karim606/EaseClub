using Serilog.Context;
using System.Diagnostics;


namespace EaseClub.Api.Infrastructure
{
    public class RequestLogContextMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = context.TraceIdentifier;
            var traceId = Activity.Current?.Id;

            using  (LogContext.PushProperty("RequestId", requestId))
            using (LogContext.PushProperty("TraceId", traceId))
            {
                
                await _next(context);
            }
        }
    }
}
