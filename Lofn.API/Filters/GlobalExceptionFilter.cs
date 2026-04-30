using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lofn.API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is UnauthorizedAccessException)
            {
                context.Result = new ForbidResult();
                context.ExceptionHandled = true;
                return;
            }

            if (context.Exception is ValidationException validationEx)
            {
                var errors = validationEx.Errors.Select(e => e.ErrorMessage).ToList();
                context.Result = new BadRequestObjectResult(new { success = false, errors });
                context.ExceptionHandled = true;
                return;
            }

            var chain = FlattenExceptionChain(context.Exception);
            var method = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path;

            // Belt-and-suspenders logging:
            // 1) ILogger<T> via Serilog (host pipeline) — primary path.
            try
            {
                _logger.LogError(context.Exception,
                    "Unhandled exception on {Method} {Path}. Chain: {Chain}",
                    method, path, string.Join(" | ", chain));
            }
            catch { /* swallow logger failures so they don't mask the original error */ }

            // 2) Static Serilog Log.Error — fires even if ILogger DI somehow fails.
            try
            {
                Log.Error(context.Exception,
                    "Unhandled exception on {Method} {Path}. Chain: {Chain}",
                    method, path, string.Join(" | ", chain));
            }
            catch { /* idem */ }

            // 3) Direct stderr write — guaranteed to land in container stdout/stderr
            //    regardless of logging provider state. Prefixed for grep-ability.
            try
            {
                Console.Error.WriteLine(
                    $"[GLOBAL-EXCEPTION] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {method} {path}{Environment.NewLine}" +
                    string.Join(Environment.NewLine, chain.Select(c => $"  └─ {c}")) +
                    Environment.NewLine +
                    $"[GLOBAL-EXCEPTION-STACK] {context.Exception}");
                Console.Error.Flush();
            }
            catch { /* idem */ }

            context.Result = new ObjectResult(new
            {
                success = false,
                error = context.Exception.Message,
                exceptionType = context.Exception.GetType().FullName,
                innerExceptions = chain
            })
            {
                StatusCode = 500
            };
            context.ExceptionHandled = true;
        }

        private static List<string> FlattenExceptionChain(Exception ex)
        {
            var chain = new List<string>();
            var current = ex;
            while (current != null)
            {
                chain.Add($"{current.GetType().Name}: {current.Message}");
                current = current.InnerException;
            }
            return chain;
        }
    }
}
