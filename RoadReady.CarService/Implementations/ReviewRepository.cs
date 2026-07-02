using Microsoft.EntityFrameworkCore;
using RoadReady.CarService.Data;
using RoadReady.CarService.Interfaces;
using RoadReady.CarService.Models;

namespace RoadReady.CarService.Implementations;

public class ReviewRepository : IReviewRepository
{
    private readonly CarDbContext _context;

    public ReviewRepository(CarDbContext context)
    {
        _context = context;
    }

    public async Task<Review> AddReviewAsync(Review review)
    {
        await _context.Reviews.AddAsync(review);
        return review;
    }

    public async Task<IEnumerable<Review>> GetReviewsByCarIdAsync(int carId)
    {
        return await _context.Reviews
            .Where(r => r.CarId == carId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetReviewByIdAsync(Guid reviewId)
    {
        return await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
    }

    public async Task<bool> HasUserReviewedCarAsync(int carId, Guid userId)
    {
        return await _context.Reviews.AnyAsync(r => r.CarId == carId && r.UserId == userId);
    }

    public Task UpdateReviewAsync(Review review)
    {
        _context.Reviews.Update(review);
        return Task.CompletedTask;
    }

    public Task DeleteReviewAsync(Review review)
    {
        _context.Reviews.Remove(review);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}