using FluentValidation;

namespace Sail.Filters;

public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().First();

        var validationResult = await validator.ValidateAsync(
            argument,
            context.HttpContext.RequestAborted);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => $"[{e.ErrorCode}] {e.ErrorMessage}").ToArray());

            return Results.ValidationProblem(
                errors,
                statusCode: StatusCodes.Status422UnprocessableEntity);
        }

        return await next(context);
    }
}
