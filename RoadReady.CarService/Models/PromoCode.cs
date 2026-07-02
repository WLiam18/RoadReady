using System.ComponentModel.DataAnnotations.Schema;
using RoadReady.Shared.DTOs.PromoCode;

namespace RoadReady.CarService.Models;

public class PromoCode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountValue { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinBookingAmount { get; set; }

    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }

    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

