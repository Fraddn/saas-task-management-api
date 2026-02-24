using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProjectSaas.Api.Application.Exceptions;

namespace ProjectSaas.Api.Common.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            await WriteProblem(context, StatusCodes.Status409Conflict, "Conflict", "Unique constraint violation.");
        }
        catch (Exception ex)
        {
            var (status, title, detail) = ex switch
            {
                InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", ex.Message),

                // Add these two:
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found", ex.Message),
                ConcurrencyConflictException => (StatusCodes.Status409Conflict, "Concurrency conflict", ex.Message),

                ArgumentException => (StatusCodes.Status400BadRequest, "Validation failed", ex.Message),
                InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict", ex.Message),

                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
            };

            await WriteProblem(context, status, title, detail);
        }
    }

    private static async Task WriteProblem(HttpContext context, int status, string title, string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = MediaTypeNames.Application.ProblemJson;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
