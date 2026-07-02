using RoadReady.CarService.Models;

namespace RoadReady.CarService.Interfaces;

public interface ICarRepository
{
    Task<List<Car>> GetAllAsync();
    Task<Car?> GetByIdAsync(int id);
    Task<List<Car>> SearchAsync(string? location, string? make, string? model, decimal? minPrice, decimal? maxPrice, string? transmission, string? fuelType, int? seatingCapacity);
    Task AddAsync(Car car);
    Task UpdateAsync(Car car);
    Task DeleteAsync(Car car);
    Task SaveAsync();
}
