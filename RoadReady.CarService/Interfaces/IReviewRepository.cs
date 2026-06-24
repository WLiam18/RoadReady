using RoadReady.CarService.Models;

namespace RoadReady.CarService.Interfaces;

public interface IReviewRepository
{
    Task<Review> AddReviewAsync(Review review);
    Task<IEnumerable<Review>> GetReviewsByCarIdAsync(int carId);
    Task<bool> HasUserReviewedCarAsync(int carId, Guid userId);
    Task SaveChangesAsync();
}