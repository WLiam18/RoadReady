using FluentValidation;
using RoadReady.Shared.DTOs.Car;

namespace RoadReady.CarService.Validators;

public class UpdateCarRequestValidator : AbstractValidator<UpdateCarRequestDto>
{
    public UpdateCarRequestValidator()
    {
        RuleFor(x => x.Location).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PricePerDay).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
