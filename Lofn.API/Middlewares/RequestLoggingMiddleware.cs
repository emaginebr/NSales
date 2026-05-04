using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Lofn.API.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var method = context.Request.Method;
            var path = context.Request.Path;
            var start = DateTime.UtcNow;

            _logger.LogInformation("HTTP {Method} {Path} started", method, path);

            try
            {
                await _next(context);
                var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                    method, path, context.Response.StatusCode, elapsed);
            }
            catch (Exception ex)
            {
                var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;

                try
                {
                    _logger.LogError(ex,
                        "HTTP {Method} {Path} threw unhandled exception after {Elapsed}ms",
                        method, path, elapsed);
                }
                catch { }

                try
                {
                    Log.Error(ex,
                        "HTTP {Method} {Path} threw unhandled exception after {Elapsed}ms",
                        method, path, elapsed);
                }
                catch { }

                try
                {
                    Console.Error.WriteLine(
                        $"[REQUEST-EXCEPTION] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {method} {path}{Environment.NewLine}{ex}");
                    Console.Error.Flush();
                }
                catch { }

                throw;
            }
        }
    }
}
