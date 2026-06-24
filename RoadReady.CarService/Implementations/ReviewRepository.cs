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

    public async Task<bool> HasUserReviewedCarAsync(int carId, Guid userId)
    {
        return await _context.Reviews.AnyAsync(r => r.CarId == carId && r.UserId == userId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}