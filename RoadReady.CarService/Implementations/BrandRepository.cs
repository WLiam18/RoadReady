using Microsoft.EntityFrameworkCore;
using RoadReady.CarService.Data;
using RoadReady.CarService.Interfaces;
using RoadReady.CarService.Models;

namespace RoadReady.CarService.Implementations;

public class BrandRepository : IBrandRepository
{
    private readonly CarDbContext _context;

    public BrandRepository(CarDbContext context)
    {
        _context = context;
    }

    public async Task<List<Brand>> GetAllAsync()
    {
        return await _context.Brands
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<Brand?> GetByIdAsync(int id)
    {
        return await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Brand?> GetByNameAsync(string name)
    {
        return await _context.Brands
            .FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower());
    }

    public async Task AddAsync(Brand brand)
    {
        await _context.Brands.AddAsync(brand);
    }

    public Task UpdateAsync(Brand brand)
    {
        _context.Brands.Update(brand);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Brand brand)
    {
        _context.Brands.Remove(brand);
        return Task.CompletedTask;
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
