using FluentValidation;
using Sail.Middleware.Models;

namespace Sail.Middleware.Validators;

public class CorsRequestValidator : AbstractValidator<CorsRequest>
{
    public CorsRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("CORS policy name is required")
            .MaximumLength(200)
            .WithMessage("CORS policy name must not exceed 200 characters");

        RuleFor(x => x.AllowOrigins)
            .Must(origins => origins == null || origins.All(o => !string.IsNullOrWhiteSpace(o)))
            .When(x => x.AllowOrigins != null && x.AllowOrigins.Any())
            .WithMessage("CORS allow origins must not contain empty values");

        RuleFor(x => x.AllowMethods)
            .Must(methods => methods == null || methods.All(m => !string.IsNullOrWhiteSpace(m)))
            .When(x => x.AllowMethods != null && x.AllowMethods.Any())
            .WithMessage("CORS allow methods must not contain empty values");

        RuleFor(x => x.AllowHeaders)
            .Must(headers => headers == null || headers.All(h => !string.IsNullOrWhiteSpace(h)))
            .When(x => x.AllowHeaders != null && x.AllowHeaders.Any())
            .WithMessage("CORS allow headers must not contain empty values");

        RuleFor(x => x.ExposeHeaders)
            .Must(headers => headers == null || headers.All(h => !string.IsNullOrWhiteSpace(h)))
            .When(x => x.ExposeHeaders != null && x.ExposeHeaders.Any())
            .WithMessage("CORS expose headers must not contain empty values");

        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxAge.HasValue)
            .WithMessage("CORS max age must be greater than or equal to 0");
    }
}

