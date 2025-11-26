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
            return Results.ValidationProblem(
                validationResult.ToDictionary(),
                statusCode: StatusCodes.Status422UnprocessableEntity);
        }

        return await next(context);
    }
}
