using FluentValidation;
using RoadReady.Shared.DTOs.Car;

namespace RoadReady.CarService.Validators;

public class CreateCarRequestValidator : AbstractValidator<CreateCarRequestDto>
{
    public CreateCarRequestValidator()
    {
        RuleFor(x => x.Make).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Year).InclusiveBetween(1990, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Color).NotEmpty().MaximumLength(30);
        RuleFor(x => x.LicensePlate).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PricePerDay).GreaterThan(0);
        RuleFor(x => x.Transmission).NotEmpty();
        RuleFor(x => x.FuelType).NotEmpty();
        RuleFor(x => x.SeatingCapacity).GreaterThan(0);
        RuleFor(x => x.BrandId).GreaterThan(0);
    }
}
