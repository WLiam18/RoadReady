using FluentValidation;
using RoadReady.Shared.DTOs.Car;

namespace RoadReady.CarService.Validators;

public class CreateBrandRequestValidator : AbstractValidator<CreateBrandRequestDto>
{
    public CreateBrandRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LogoUrl).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(50);
    }
}
