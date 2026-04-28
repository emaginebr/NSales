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
        _logger.LogError(exception,
            "GraphQL request error. Document: {Document}",
            context.Document?.ToString(indented: false) ?? "(none)");
    }

    public override void ValidationErrors(IRequestContext context, IReadOnlyList<IError> errors)
    {
        foreach (var error in errors)
        {
            var path = error.Path?.ToString() ?? "(root)";
            _logger.LogWarning(error.Exception,
                "GraphQL validation error at {Path}: {Message}",
                path, error.Message);
        }
    }

    public override void SyntaxError(IRequestContext context, IError error)
    {
        _logger.LogWarning(error.Exception,
            "GraphQL syntax error: {Message}",
            error.Message);
    }

    public override void ResolverError(IMiddlewareContext context, IError error)
    {
        var ex = error.Exception;
        if (ex is not null)
        {
            _logger.LogError(ex,
                "GraphQL resolver error at {Path}: {Message}. Field: {Field}. Type: {Type}. Coordinate: {Coordinate}. Exception: {ExceptionType}",
                context.Path,
                error.Message,
                context.Selection.Field.Name,
                context.Selection.Field.DeclaringType.Name,
                context.Selection.Field.Coordinate,
                ex.GetType().FullName);
        }
        else
        {
            _logger.LogError(
                "GraphQL resolver error at {Path}: {Message}. Field: {Field}. Type: {Type}. (no exception attached)",
                context.Path,
                error.Message,
                context.Selection.Field.Name,
                context.Selection.Field.DeclaringType.Name);
        }
    }

    public override void ResolverError(IRequestContext context, ISelection selection, IError error)
    {
        var ex = error.Exception;
        if (ex is not null)
        {
            _logger.LogError(ex,
                "GraphQL resolver error on field {Field} of {Type}: {Message}. Exception: {ExceptionType}",
                selection.Field.Name,
                selection.Field.DeclaringType.Name,
                error.Message,
                ex.GetType().FullName);
        }
        else
        {
            _logger.LogError(
                "GraphQL resolver error on field {Field} of {Type}: {Message}. (no exception attached)",
                selection.Field.Name,
                selection.Field.DeclaringType.Name,
                error.Message);
        }
    }

    public override void TaskError(IExecutionTask task, IError error)
    {
        var path = error.Path?.ToString() ?? "(root)";
        var ex = error.Exception;
        if (ex is not null)
        {
            _logger.LogError(ex,
                "GraphQL task error at {Path}: {Message}. Exception: {ExceptionType}",
                path, error.Message, ex.GetType().FullName);
        }
        else
        {
            _logger.LogError(
                "GraphQL task error at {Path}: {Message}. (no exception attached)",
                path, error.Message);
        }
    }
}
