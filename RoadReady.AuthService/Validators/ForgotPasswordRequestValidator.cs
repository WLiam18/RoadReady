using FluentValidation;
using RoadReady.Shared.DTOs.Auth;

namespace RoadReady.AuthService.Validators;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");
    }
}
