using RoadReady.CarService.Models;

namespace RoadReady.CarService.Interfaces;

public interface IReviewRepository
{
    Task<Review> AddReviewAsync(Review review);
    Task<IEnumerable<Review>> GetReviewsByCarIdAsync(int carId);
    Task<Review?> GetReviewByIdAsync(Guid reviewId);
    Task<bool> HasUserReviewedCarAsync(int carId, Guid userId);
    Task UpdateReviewAsync(Review review);
    Task DeleteReviewAsync(Review review);
    Task SaveChangesAsync();
}