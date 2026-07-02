using RoadReady.Shared.Enums;

namespace RoadReady.Shared.DTOs.PromoCode;

public class PromoCodeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinBookingAmount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePromoCodeRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinBookingAmount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ValidatePromoRequestDto
{
    public string Code { get; set; } = string.Empty;
    public decimal BookingAmount { get; set; }
}

public class ValidatePromoResponseDto
{
    public bool IsValid { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public DiscountType DiscountType { get; set; }
    public string Message { get; set; } = string.Empty;
}

public enum DiscountType
{
    Percentage,
    FlatAmount
}
