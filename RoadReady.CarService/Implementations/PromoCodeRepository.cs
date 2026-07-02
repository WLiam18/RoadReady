using Microsoft.EntityFrameworkCore;
using RoadReady.CarService.Data;
using RoadReady.CarService.Interfaces;
using RoadReady.CarService.Models;

namespace RoadReady.CarService.Implementations;

public class PromoCodeRepository : IPromoCodeRepository
{
    private readonly CarDbContext _context;

    public PromoCodeRepository(CarDbContext context)
    {
        _context = context;
    }

    public async Task<List<PromoCode>> GetAllAsync()
    {
        return await _context.PromoCodes
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<PromoCode?> GetByIdAsync(int id)
    {
        return await _context.PromoCodes.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PromoCode?> GetByCodeAsync(string code)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await _context.PromoCodes.FirstOrDefaultAsync(p => p.Code.ToUpper() == normalized);
    }

    public async Task AddAsync(PromoCode promo)
    {
        await _context.PromoCodes.AddAsync(promo);
    }

    public Task UpdateAsync(PromoCode promo)
    {
        _context.PromoCodes.Update(promo);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(PromoCode promo)
    {
        _context.PromoCodes.Remove(promo);
        return Task.CompletedTask;
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
