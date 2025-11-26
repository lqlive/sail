using Sail.Filters;

namespace Sail.Extensions;

public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder AddRequestValidation<T>(
        this RouteHandlerBuilder routeBuilder)
    {
        return routeBuilder
            .AddEndpointFilter<ValidationFilter<T>>()
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }
}
