using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Execution.Processing;
using HotChocolate.Resolvers;
using Microsoft.Extensions.Logging;

namespace Lofn.GraphQL;

public class GraphQLErrorLogger : ExecutionDiagnosticEventListener
{
    private readonly ILogger<GraphQLErrorLogger> _logger;

    public GraphQLErrorLogger(ILogger<GraphQLErrorLogger> logger)
    {
        _logger = logger;
    }

    public override void RequestError(IRequestContext context, Exception exception)
    {
        _logger.LogError(exception, "GraphQL request error");
    }

    public override void ValidationErrors(IRequestContext context, IReadOnlyList<IError> errors)
    {
        foreach (var error in errors)
            _logger.LogWarning("GraphQL validation error: {Message}", error.Message);
    }

    public override void SyntaxError(IRequestContext context, IError error)
    {
        _logger.LogWarning("GraphQL syntax error: {Message}", error.Message);
    }

    public override void ResolverError(IMiddlewareContext context, IError error)
    {
        _logger.LogError(error.Exception, "GraphQL resolver error on {Path}: {Message}", context.Path, error.Message);
    }

    public override void ResolverError(IRequestContext context, ISelection selection, IError error)
    {
        _logger.LogError(error.Exception, "GraphQL resolver error on {Field}: {Message}", selection.Field.Name, error.Message);
    }
}
