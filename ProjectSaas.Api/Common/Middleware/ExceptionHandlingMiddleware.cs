using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

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
        catch (ArgumentException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Validation failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteProblem(context, StatusCodes.Status409Conflict, "Conflict", ex.Message);
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
