using StorageSystem.Application.Exceptions;
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
            ApplicationValidationException validationException => CreateProblemDetails(
                StatusCodes.Status422UnprocessableEntity,
                "One or more validation errors occurred.",
                "ValidationError",
                validationException.Message,
                validationException.Errors
            ),
            NotFoundException => CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Not Found",
                "NotFound",
                exception.Message
            ),
            ConflictException => CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Conflict",
                "Conflict",
                exception.Message
            ),
            _ => CreateProblemDetails(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                "UnexpectedError",
                exception.Message
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
        IReadOnlyCollection<string>? errors = null
    )
    {
        var details = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = detail
        };

        if (errors is not null)
        {
            details.Extensions["errors"] = errors;
        }

        return details;
    }
}
