using FluentValidation;
using RoadReady.Shared.DTOs.Auth;

namespace RoadReady.AuthService.Validators;

public class UpdatePasswordRequestValidator : AbstractValidator<UpdatePasswordRequestDto>
{
    public UpdatePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters long.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Confirm new password is required.")
            .Equal(x => x.NewPassword).WithMessage("New password and confirm password must match.");
    }
}
