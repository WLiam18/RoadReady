using RoadReady.CarService.Models;

namespace RoadReady.CarService.Interfaces;

public interface IBrandRepository
{
    Task<List<Brand>> GetAllAsync();
    Task<Brand?> GetByIdAsync(int id);
    Task<Brand?> GetByNameAsync(string name);
    Task AddAsync(Brand brand);
    Task UpdateAsync(Brand brand);
    Task DeleteAsync(Brand brand);
    Task SaveAsync();
}
