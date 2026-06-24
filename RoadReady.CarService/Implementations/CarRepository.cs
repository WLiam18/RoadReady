using Microsoft.EntityFrameworkCore;
using RoadReady.CarService.Data;
using RoadReady.CarService.Interfaces;
using RoadReady.CarService.Models;

namespace RoadReady.CarService.Implementations;

public class CarRepository : ICarRepository
{
    private readonly CarDbContext _context;

    public CarRepository(CarDbContext context)
    {
        _context = context;
    }

    public async Task<List<Car>> GetAllAsync()
    {
        return await _context.Cars
            .Include(c => c.Brand)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Car?> GetByIdAsync(int id)
    {
        return await _context.Cars
            .Include(c => c.Brand)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Car>> SearchAsync(string? location, string? make, string? model)
    {
        var query = _context.Cars
            .Include(c => c.Brand)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(c => c.Location.Contains(location));
        }

        if (!string.IsNullOrWhiteSpace(make))
        {
            query = query.Where(c => c.Make.Contains(make));
        }

        if (!string.IsNullOrWhiteSpace(model))
        {
            query = query.Where(c => c.Model.Contains(model));
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Car car)
    {
        await _context.Cars.AddAsync(car);
    }

    public Task UpdateAsync(Car car)
    {
        _context.Cars.Update(car);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Car car)
    {
        _context.Cars.Remove(car);
        return Task.CompletedTask;
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
