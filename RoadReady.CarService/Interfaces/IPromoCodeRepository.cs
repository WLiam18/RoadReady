using RoadReady.CarService.Models;

namespace RoadReady.CarService.Interfaces;

public interface IPromoCodeRepository
{
    Task<List<PromoCode>> GetAllAsync();
    Task<PromoCode?> GetByIdAsync(int id);
    Task<PromoCode?> GetByCodeAsync(string code);
    Task AddAsync(PromoCode promo);
    Task UpdateAsync(PromoCode promo);
    Task DeleteAsync(PromoCode promo);
    Task SaveAsync();
}
