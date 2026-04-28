using System;
using System.Linq;
using HotChocolate;
using Microsoft.Extensions.Logging;

namespace Lofn.GraphQL;

public class GraphQLErrorFilter : IErrorFilter
{
    private static int _instanceCount;
    private readonly ILogger<GraphQLErrorFilter> _logger;

    public GraphQLErrorFilter(ILogger<GraphQLErrorFilter> logger)
    {
        _logger = logger;
        var n = System.Threading.Interlocked.Increment(ref _instanceCount);

        var msg = $"[GraphQLErrorFilter] CONSTRUCTED instance #{n} at {DateTime.UtcNow:O}";
        Console.Error.WriteLine(msg);
        Console.Out.WriteLine(msg);
        _logger.LogInformation("GraphQLErrorFilter constructed (instance #{Instance})", n);
    }

    public IError OnError(IError error)
    {
        var path = error.Path?.ToString() ?? "(root)";
        var locations = error.Locations is { Count: > 0 }
            ? string.Join(",", error.Locations.Select(l => $"L{l.Line}:C{l.Column}"))
            : "(none)";

        var ex = error.Exception;
        var exceptionType = ex?.GetType().FullName ?? "(none)";
        var exceptionMessage = ex?.Message ?? "(no exception)";
        var stackTrace = ex?.ToString() ?? "(no stack trace)";

        var headline =
            $"[GraphQLErrorFilter] {DateTime.UtcNow:O} path={path} loc={locations} " +
            $"code={error.Code ?? "(none)"} message=\"{error.Message}\" exType={exceptionType} " +
            $"exMessage=\"{exceptionMessage}\"";

        Console.Error.WriteLine(headline);
        Console.Error.WriteLine(stackTrace);
        Console.Out.WriteLine(headline);

        if (ex is not null)
        {
            _logger.LogError(ex,
                "GraphQL error at {Path} [{Locations}]: {Message}. Code: {Code}. Exception: {ExceptionType}",
                path, locations, error.Message, error.Code ?? "(none)", exceptionType);
        }
        else
        {
            _logger.LogError(
                "GraphQL error at {Path} [{Locations}]: {Message}. Code: {Code}. (no exception attached)",
                path, locations, error.Message, error.Code ?? "(none)");
        }

        return error;
    }
}
