using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Domain.Commons;

namespace ModernPaySystem.Infrastructure.Extensions;

/// <summary>
/// Extension methods for converting Result<T> to IActionResult.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an IActionResult with appropriate HTTP status codes.
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result) where T : notnull
    {
        if (result.IsError)
        {
            var error = result.TopError;
            return error.Type switch
            {
                ErrorKind.NotFound => new NotFoundObjectResult(new { errors = result.Errors }),
                ErrorKind.Unauthorized => new UnauthorizedObjectResult(new { errors = result.Errors }),
                ErrorKind.Forbidden => new ObjectResult(new { errors = result.Errors }) { StatusCode = 403 },
                ErrorKind.Conflict => new ConflictObjectResult(new { errors = result.Errors }),
                ErrorKind.Validation => new BadRequestObjectResult(new { errors = result.Errors }),
                _ => new BadRequestObjectResult(new { errors = result.Errors })
            };
        }

        return result.Value switch
        {
            Created created => new CreatedResult("/", new { data = created.Data }),
            Deleted => new NoContentResult(),
            Updated updated => new OkObjectResult(new { data = updated.Data }),
            Success success => new OkObjectResult(new { data = success.Data }),
            _ => new OkObjectResult(new { data = result.Value })
        };
    }

    /// <summary>
    /// Converts a Result to an IActionResult with a specific location header for Created responses
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result, string? locationUri = null) where T : notnull
    {
        if (result.IsError)
        {
            var error = result.TopError;
            return error.Type switch
            {
                ErrorKind.NotFound => new NotFoundObjectResult(new { errors = result.Errors }),
                ErrorKind.Unauthorized => new UnauthorizedObjectResult(new { errors = result.Errors }),
                ErrorKind.Forbidden => new ObjectResult(new { errors = result.Errors }) { StatusCode = 403 },
                ErrorKind.Conflict => new ConflictObjectResult(new { errors = result.Errors }),
                ErrorKind.Validation => new BadRequestObjectResult(new { errors = result.Errors }),
                _ => new BadRequestObjectResult(new { errors = result.Errors })
            };
        }

        return result.Value switch
        {
            Created created => new CreatedResult(locationUri ?? "/", new { data = created.Data }),
            Deleted => new NoContentResult(),
            Updated updated => new OkObjectResult(new { data = updated.Data }),
            Success success => new OkObjectResult(new { data = success.Data }),
            _ => new OkObjectResult(new { data = result.Value })
        };
    }

    /// <summary>
    /// Converts a Result to an IActionResult with action name and route values for Created responses
    /// </summary>
    public static IActionResult ToCreatedAtActionResult<T>(this Result<T> result, ControllerBase controller, string actionName, object? routeValues = null) where T : notnull
    {
        if (result.IsError)
        {
            var error = result.TopError;
            return error.Type switch
            {
                ErrorKind.NotFound => new NotFoundObjectResult(new { errors = result.Errors }),
                ErrorKind.Unauthorized => new UnauthorizedObjectResult(new { errors = result.Errors }),
                ErrorKind.Forbidden => new ObjectResult(new { errors = result.Errors }) { StatusCode = 403 },
                ErrorKind.Conflict => new ConflictObjectResult(new { errors = result.Errors }),
                ErrorKind.Validation => new BadRequestObjectResult(new { errors = result.Errors }),
                _ => new BadRequestObjectResult(new { errors = result.Errors })
            };
        }

        return result.Value switch
        {
            Created created => controller.CreatedAtAction(actionName, routeValues, new { data = created.Data }),
            Deleted => new NoContentResult(),
            Updated updated => new OkObjectResult(new { data = updated.Data }),
            Success success => new OkObjectResult(new { data = success.Data }),
            _ => new OkObjectResult(new { data = result.Value })
        };
    }
}
