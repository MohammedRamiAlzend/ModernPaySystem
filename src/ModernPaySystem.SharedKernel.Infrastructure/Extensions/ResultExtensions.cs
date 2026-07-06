using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.SharedKernel.Infrastructure.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
        where T : notnull
    {
        if (result.IsError)
        {
            var error = result.TopError;
            return error.Type switch
            {
                ErrorKind.NotFound => new NotFoundObjectResult(new { errors = result.Errors }),
                ErrorKind.Unauthorized => new ObjectResult(new { errors = result.Errors }) { StatusCode = 401 },
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
}
