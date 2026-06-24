using FluentValidation;
using RoadReady.Shared.DTOs.Booking;

namespace RoadReady.BookingService.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequestDto>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.CarId).GreaterThan(0);
        RuleFor(x => x.PickupLocation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PickupDate).GreaterThan(DateTime.UtcNow.AddMinutes(-1));
        RuleFor(x => x.DropoffDate).GreaterThan(x => x.PickupDate);
    }
}
