using StorageSystem.Application.Exceptions;
using StorageSystem.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StorageSystem.Api.Filters;

public class ApiGlobalExceptionFilter(IHostEnvironment environment) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;

        var details = exception switch
        {
            EntityValidationException ex => CreateProblemDetails(
                StatusCodes.Status422UnprocessableEntity,
                "One or more validation errors occurred.",
                "EntityValidationError",
                ex.Message,
                ex.Errors?.Select(error => error.Message).ToArray()
            ),
            ApplicationValidationException ex => CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "One or more validation errors occurred.",
                "ValidationError",
                ex.Message,
                ex.Errors
            ),
            NotFoundException ex => CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Not Found",
                "NotFound",
                ex.Message
            ),
            ConflictException ex => CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Conflict",
                "Conflict",
                ex.Message
            ),
            UnauthorizedAccessException ex => CreateProblemDetails(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "Unauthorized",
                ex.Message
            ),
            _ => CreateProblemDetails(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred on the server.",
                "InternalServerError",
                environment.IsDevelopment() ? exception.Message : "Please contact support."
            )
        };

        if (environment.IsDevelopment())
        {
            details.Extensions["stackTrace"] = exception.StackTrace;
        }

        context.HttpContext.Response.StatusCode = details.Status ?? StatusCodes.Status500InternalServerError;
        context.Result = new ObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static ProblemDetails CreateProblemDetails(
        int statusCode,
        string title,
        string type,
        string detail,
        IReadOnlyCollection<string>? errors = null)
    {
        var details = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = detail
        };

        if (errors is not null && errors.Count > 0)
        {
            details.Extensions["errors"] = errors;
        }

        return details;
    }
}