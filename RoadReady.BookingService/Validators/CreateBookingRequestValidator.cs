using FluentValidation;
using RoadReady.Shared.DTOs.Booking;
using RoadReady.Shared.Enums;

namespace RoadReady.BookingService.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequestDto>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.CarId).GreaterThan(0);
        RuleFor(x => x.PickupLocation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PickupDate).GreaterThan(DateTime.UtcNow.AddMinutes(-1)).WithMessage("Pick-up date/time must be in the future.");
        RuleFor(x => x.PickupTime).NotEmpty().WithMessage("Pick-up time is required.");
        RuleFor(x => x.DropoffTime).NotEmpty().WithMessage("Drop-off time is required.");
        RuleFor(x => x).Must(x =>
        {
            if (!TimeSpan.TryParse(x.PickupTime, out var pt)) return false;
            if (!TimeSpan.TryParse(x.DropoffTime, out var dt)) return false;
            var pickup = x.PickupDate.Date + pt;
            var dropoff = x.DropoffDate.Date + dt;
            if (pickup.Date == dropoff.Date) return dt > pt;
            return dropoff > pickup;
        }).WithMessage("Drop-off date+time must be after pick-up date+time.");
    }
}
