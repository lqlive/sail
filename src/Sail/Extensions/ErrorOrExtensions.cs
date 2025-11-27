using ErrorOr;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Sail.Extensions;

public static class ErrorOrExtensions
{
    public static ProblemHttpResult HandleErrors(this List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return TypedResults.Problem();
        }

        var error = errors[0];
        var statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };
        return TypedResults.Problem(detail: error.Description, statusCode: statusCode);
    }
}
