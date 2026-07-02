using RoadReady.CarService.Interfaces;
using RoadReady.CarService.Models;
using RoadReady.Shared.DTOs.Car;
using RoadReady.Shared.Responses;

namespace RoadReady.CarService.Implementations;

public class CarService : ICarService
{
    private readonly ICarRepository _carRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly ILogger<CarService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public CarService(
        ICarRepository carRepository, 
        IBrandRepository brandRepository, 
        ILogger<CarService> logger,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _carRepository = carRepository;
        _brandRepository = brandRepository;
        _logger = logger;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<PagedResponse<CarDto>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var cars = await _carRepository.GetAllAsync();

        var totalCount = cars.Count;

        var pagedCars = cars
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapCarToDto)
            .ToList();

        return PagedResponse<CarDto>.Create(pagedCars, page, pageSize, totalCount);
    }

    public async Task<ApiResponse<CarDto>> GetByIdAsync(int id)
    {
        var car = await _carRepository.GetByIdAsync(id);

        if (car == null)
        {
            return ApiResponse<CarDto>.Fail("Car not found.");
        }

        return ApiResponse<CarDto>.Ok(MapCarToDto(car), "Car fetched successfully.");
    }

    public async Task<PagedResponse<CarDto>> SearchAsync(CarSearchRequestDto request)
    {
        var cars = await _carRepository.SearchAsync(
            request.Location, request.Make, request.Model,
            request.MinPrice, request.MaxPrice,
            request.Transmission, request.FuelType,
            request.SeatingCapacity);

        if (request.PickupDate.HasValue && request.DropoffDate.HasValue)
        {
            try
            {
                var bookingServiceUrl = _configuration["Services:BookingServiceBaseUrl"] ?? "http://localhost:5003";
                var verifyUrl = $"{bookingServiceUrl.TrimEnd('/')}/api/v1/bookings/unavailable-car-ids?pickupDate={request.PickupDate:O}&dropoffDate={request.DropoffDate:O}";
                var httpResponse = await _httpClient.GetAsync(verifyUrl);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var unavailableCarIds = await httpResponse.Content.ReadFromJsonAsync<List<int>>();
                    if (unavailableCarIds != null)
                    {
                        cars = cars.Where(c => !unavailableCarIds.Contains(c.Id)).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch unavailable car IDs from BookingService.");
            }
        }

        var totalCount = cars.Count;

        var pagedCars = cars
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapCarToDto)
            .ToList();

        var response = PagedResponse<CarDto>.Create(pagedCars, request.Page, request.PageSize, totalCount);

        if (totalCount == 0)
        {
            bool hasLocation = !string.IsNullOrWhiteSpace(request.Location);
            bool hasMake = !string.IsNullOrWhiteSpace(request.Make);
            bool hasModel = !string.IsNullOrWhiteSpace(request.Model);

            if (hasMake && hasModel && hasLocation)
                response.Message = $"We currently do not have any {request.Make} {request.Model} cars available in {request.Location}.";
            else if (hasMake && hasLocation)
                response.Message = $"We currently do not have any {request.Make} cars available in {request.Location}.";
            else if (hasMake && hasModel)
                response.Message = $"We currently do not have any {request.Make} {request.Model} cars available.";
            else if (hasMake)
                response.Message = $"We currently do not have any {request.Make} cars available.";
            else if (hasLocation)
                response.Message = $"There are no cars available in {request.Location}. Hang tight, we are expanding our services!";
            else
                response.Message = "We currently do not have any cars matching your search criteria.";
        }

        return response;
    }

    public async Task<ApiResponse<CarDto>> CreateAsync(CreateCarRequestDto request)
    {
        var brand = await _brandRepository.GetByIdAsync(request.BrandId);

        if (brand == null)
        {
            return ApiResponse<CarDto>.Fail("Selected brand does not exist.");
        }

        var car = new Car
        {
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            Color = request.Color,
            LicensePlate = request.LicensePlate,
            Location = request.Location,
            PricePerDay = request.PricePerDay,
            Transmission = request.Transmission,
            FuelType = request.FuelType,
            SeatingCapacity = request.SeatingCapacity,
            Description = request.Description,
            ImageUrls = string.Join(",", request.ImageUrls),
            BrandId = request.BrandId
        };

        await _carRepository.AddAsync(car);
        await _carRepository.SaveAsync();

        car.Brand = brand;

        _logger.LogInformation("Car created successfully with license plate: {LicensePlate}", car.LicensePlate);

        return ApiResponse<CarDto>.Created(MapCarToDto(car), "Car created successfully.");
    }

    public async Task<ApiResponse<CarDto>> UpdateAsync(int id, UpdateCarRequestDto request)
    {
        var car = await _carRepository.GetByIdAsync(id);

        if (car == null)
        {
            return ApiResponse<CarDto>.Fail("Car not found.");
        }

        car.Location = request.Location;
        car.PricePerDay = request.PricePerDay;
        car.Description = request.Description;
        car.ImageUrls = string.Join(",", request.ImageUrls);
        car.Status = request.Status;
        car.UpdatedAt = DateTime.UtcNow;

        await _carRepository.UpdateAsync(car);
        await _carRepository.SaveAsync();

        _logger.LogInformation("Car updated successfully with id: {CarId}", car.Id);

        return ApiResponse<CarDto>.Ok(MapCarToDto(car), "Car updated successfully.");
    }

    public async Task<ApiResponse<string>> DeleteAsync(int id)
    {
        var car = await _carRepository.GetByIdAsync(id);

        if (car == null)
        {
            return ApiResponse<string>.Fail("Car not found.");
        }

        await _carRepository.DeleteAsync(car);
        await _carRepository.SaveAsync();

        _logger.LogInformation("Car deleted successfully with id: {CarId}", car.Id);

        return ApiResponse<string>.Ok("Car deleted successfully.");
    }

    private static CarDto MapCarToDto(Car car)
    {
        return new CarDto
        {
            Id = car.Id,
            Make = car.Make,
            Model = car.Model,
            Year = car.Year,
            Color = car.Color,
            LicensePlate = car.LicensePlate,
            Location = car.Location,
            PricePerDay = car.PricePerDay,
            Transmission = car.Transmission,
            FuelType = car.FuelType,
            SeatingCapacity = car.SeatingCapacity,
            Description = car.Description,
            ImageUrls = string.IsNullOrWhiteSpace(car.ImageUrls)
                ? new List<string>()
                : car.ImageUrls.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            Status = car.Status,
            BrandId = car.BrandId,
            BrandName = car.Brand?.Name ?? string.Empty,
            AverageRating = car.Reviews != null && car.Reviews.Count != 0
                ? Math.Round(car.Reviews.Average(r => r.Rating), 1)
                : 0,
            ReviewCount = car.Reviews?.Count ?? 0
        };
    }
}