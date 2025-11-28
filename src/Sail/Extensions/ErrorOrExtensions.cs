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

        var validationErrors = errors.Where(e => e.Type == ErrorType.Validation).ToList();
        
        if (validationErrors.Count > 0)
        {
            var errorDictionary = validationErrors
                .GroupBy(e => e.Code.Split('.')[0])
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => $"[{e.Code}] {e.Description}").ToArray());

            return TypedResults.Problem(
                detail: string.Join("; ", validationErrors.Select(e => $"[{e.Code}] {e.Description}")),
                statusCode: StatusCodes.Status422UnprocessableEntity,
                extensions: new Dictionary<string, object?>
                {
                    ["errors"] = errorDictionary
                });
        }

        var error = errors[0];
        var statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };
        
        return TypedResults.Problem(
            title: error.Code,
            detail: error.Description,
            statusCode: statusCode);
    }
}
