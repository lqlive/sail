using FluentValidation;
using Sail.Middleware.Models;
using Sail.Middleware.Errors;

namespace Sail.Middleware.Validators;

public class CorsRequestValidator : AbstractValidator<CorsRequest>
{
    public CorsRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(CorsErrors.NameRequired.Description)
            .WithErrorCode(CorsErrors.NameRequired.Code)
            .MaximumLength(200)
            .WithMessage(CorsErrors.NameTooLong.Description)
            .WithErrorCode(CorsErrors.NameTooLong.Code);

        RuleFor(x => x.AllowOrigins)
            .Must(origins => origins == null || origins.All(o => !string.IsNullOrWhiteSpace(o)))
            .When(x => x.AllowOrigins != null && x.AllowOrigins.Any())
            .WithMessage(CorsErrors.AllowOriginsContainEmpty.Description)
            .WithErrorCode(CorsErrors.AllowOriginsContainEmpty.Code);

        RuleFor(x => x.AllowMethods)
            .Must(methods => methods == null || methods.All(m => !string.IsNullOrWhiteSpace(m)))
            .When(x => x.AllowMethods != null && x.AllowMethods.Any())
            .WithMessage(CorsErrors.AllowMethodsContainEmpty.Description)
            .WithErrorCode(CorsErrors.AllowMethodsContainEmpty.Code);

        RuleFor(x => x.AllowHeaders)
            .Must(headers => headers == null || headers.All(h => !string.IsNullOrWhiteSpace(h)))
            .When(x => x.AllowHeaders != null && x.AllowHeaders.Any())
            .WithMessage(CorsErrors.AllowHeadersContainEmpty.Description)
            .WithErrorCode(CorsErrors.AllowHeadersContainEmpty.Code);

        RuleFor(x => x.ExposeHeaders)
            .Must(headers => headers == null || headers.All(h => !string.IsNullOrWhiteSpace(h)))
            .When(x => x.ExposeHeaders != null && x.ExposeHeaders.Any())
            .WithMessage(CorsErrors.ExposeHeadersContainEmpty.Description)
            .WithErrorCode(CorsErrors.ExposeHeadersContainEmpty.Code);

        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxAge.HasValue)
            .WithMessage(CorsErrors.MaxAgeInvalid.Description)
            .WithErrorCode(CorsErrors.MaxAgeInvalid.Code);
    }
}
