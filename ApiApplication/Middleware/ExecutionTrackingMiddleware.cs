using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace ApiApplication.Middleware
{
    public class ExecutionTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExecutionTrackingMiddleware> _logger;

        public ExecutionTrackingMiddleware(RequestDelegate next, ILogger<ExecutionTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                await _next(context);
            }
            finally
            { 
                stopwatch.Stop();
                _logger.LogDebug("Call to {endpoint} took {miliseconds} ms",
                                 context.Request.GetDisplayUrl(),
                                 stopwatch.Elapsed.TotalMilliseconds);
            }
        }
    }
}
