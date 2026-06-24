using FluentValidation;
using Microsoft.AspNetCore.Http;
using RoadReady.Shared.DTOs.Booking;

namespace RoadReady.BookingService.Validators;

// 1. The Main DTO Validator
public class CreateInspectionRequestDtoValidator : AbstractValidator<CreateInspectionRequestDto>
{
    public CreateInspectionRequestDtoValidator()
    {
        // Basic Property Validation
        RuleFor(x => x.OdometerReading)
            .GreaterThanOrEqualTo(0).WithMessage("Odometer reading cannot be negative.");

        RuleFor(x => x.FuelLevel)
            .NotEmpty().WithMessage("Fuel level is required.");

        // Validate the Vehicle Images (if they exist) using our custom FileValidator below
        RuleForEach(x => x.VehicleImages)
            .SetValidator(new FormFileValidator()!)
            .When(x => x.VehicleImages != null && x.VehicleImages.Any());

        // Validate the Document Images (if they exist)
        RuleForEach(x => x.DocumentImages)
            .SetValidator(new FormFileValidator()!)
            .When(x => x.DocumentImages != null && x.DocumentImages.Any());
    }
}

// 2. The Reusable File Validator
public class FormFileValidator : AbstractValidator<IFormFile>
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
    private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/png", "image/webp", "application/pdf" };
    private const int MaxFileSize = 5 * 1024 * 1024; // 5MB

    public FormFileValidator()
    {
        RuleFor(file => file.Length)
            .GreaterThan(0).WithMessage("File cannot be empty.")
            .LessThanOrEqualTo(MaxFileSize).WithMessage("File is too large. Maximum allowed size is 5MB.");

        RuleFor(file => file.FileName)
            .Must(HaveValidExtension).WithMessage("Invalid file extension. Only JPG, PNG, WEBP, and PDF are allowed.");

        RuleFor(file => file.ContentType)
            .Must(HaveValidMimeType).WithMessage("Invalid file content type. The file might be corrupted or disguised.");
    }

    private bool HaveValidExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return false;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return _allowedExtensions.Contains(ext);
    }

    private bool HaveValidMimeType(string contentType)
    {
        if (string.IsNullOrEmpty(contentType)) return false;
        return _allowedMimeTypes.Contains(contentType.ToLowerInvariant());
    }
}