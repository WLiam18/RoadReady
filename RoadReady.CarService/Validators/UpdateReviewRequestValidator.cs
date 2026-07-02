using FluentValidation;
using RoadReady.Shared.DTOs.Car;

namespace RoadReady.CarService.Validators;

public class UpdateReviewRequestValidator : AbstractValidator<UpdateReviewRequestDto>
{
    public UpdateReviewRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters.");
    }
}