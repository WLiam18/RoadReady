using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RoadReady.CarService.Implementations;
using RoadReady.CarService.Interfaces;
using RoadReady.CarService.Models;
using RoadReady.Shared.DTOs.Car;
using RoadReady.Shared.Enums;
using System.Net.Http;

namespace RoadReady.CarService.Tests;

[TestFixture]
public class CarServiceTests
{
    private Mock<ICarRepository> _mockCarRepository;
    private Mock<IBrandRepository> _mockBrandRepository;
    private Mock<ILogger<Implementations.CarService>> _mockLogger;
    private HttpClient _httpClient;
    private IConfiguration _configuration;
    
    private Implementations.CarService _carService;

    [SetUp]
    public void Setup()
    {
        _mockCarRepository = new Mock<ICarRepository>();
        _mockBrandRepository = new Mock<IBrandRepository>();
        _mockLogger = new Mock<ILogger<Implementations.CarService>>();
        _httpClient = new HttpClient();
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "Services:BookingServiceBaseUrl", "http://localhost:5003" }
        }).Build();

        _carService = new Implementations.CarService(
            _mockCarRepository.Object,
            _mockBrandRepository.Object,
            _mockLogger.Object,
            _httpClient,
            _configuration
        );
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    [Test]
    public async Task GetByIdAsync_WhenCarExists_ReturnsSuccess()
    {
        var carId = 1;
        var existingCar = new Car
        {
            Id = carId,
            Make = "Toyota",
            Model = "Innova Crysta",
            BrandId = 1,
            Brand = new Brand { Name = "Toyota" }
        };

        _mockCarRepository.Setup(repo => repo.GetByIdAsync(carId))
                          .ReturnsAsync(existingCar);

        var result = await _carService.GetByIdAsync(carId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Make, Is.EqualTo("Toyota"));
        Assert.That(result.Data.BrandName, Is.EqualTo("Toyota"));
    }

    [Test]
    public async Task GetByIdAsync_WhenCarDoesNotExist_ReturnsFail()
    {
        var carId = 999;
        _mockCarRepository.Setup(repo => repo.GetByIdAsync(carId))
                          .ReturnsAsync((Car?)null);

        var result = await _carService.GetByIdAsync(carId);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Car not found."));
    }

    [Test]
    public async Task CreateAsync_WhenBrandExists_ReturnsCreatedSuccess()
    {
        var request = new CreateCarRequestDto
        {
            Make = "Hyundai",
            Model = "Creta",
            Year = 2024,
            LicensePlate = "TN-01-AB-1234",
            BrandId = 2
        };

        var existingBrand = new Brand { Id = 2, Name = "Hyundai" };

        _mockBrandRepository.Setup(repo => repo.GetByIdAsync(request.BrandId))
                            .ReturnsAsync(existingBrand);

        var result = await _carService.CreateAsync(request);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("Car created successfully."));
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Make, Is.EqualTo("Hyundai"));

        _mockCarRepository.Verify(repo => repo.AddAsync(It.IsAny<Car>()), Times.Once);
        _mockCarRepository.Verify(repo => repo.SaveAsync(), Times.Once);
    }

    [Test]
    public async Task CreateAsync_WhenBrandDoesNotExist_ReturnsFail()
    {
        var request = new CreateCarRequestDto
        {
            Make = "Ford",
            Model = "Endeavour",
            BrandId = 99 
        };

        _mockBrandRepository.Setup(repo => repo.GetByIdAsync(request.BrandId))
                            .ReturnsAsync((Brand?)null);

        var result = await _carService.CreateAsync(request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Selected brand does not exist."));
        _mockCarRepository.Verify(repo => repo.AddAsync(It.IsAny<Car>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_WhenCarExists_UpdatesAndReturnsSuccess()
    {
        var carId = 1;
        var request = new UpdateCarRequestDto
        {
            PricePerDay = 3500,
            Location = "Chennai Airport"
        };

        var existingCar = new Car { Id = carId, PricePerDay = 3000, Location = "Old Lot" };

        _mockCarRepository.Setup(repo => repo.GetByIdAsync(carId))
                          .ReturnsAsync(existingCar);

        var result = await _carService.UpdateAsync(carId, request);

        Assert.That(result.Success, Is.True);
        Assert.That(existingCar.PricePerDay, Is.EqualTo(3500));
        Assert.That(existingCar.Location, Is.EqualTo("Chennai Airport"));
        
        _mockCarRepository.Verify(repo => repo.UpdateAsync(existingCar), Times.Once);
        _mockCarRepository.Verify(repo => repo.SaveAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenCarDoesNotExist_ReturnsFail()
    {
        var request = new UpdateCarRequestDto { PricePerDay = 3500 };

        _mockCarRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                          .ReturnsAsync((Car?)null);

        var result = await _carService.UpdateAsync(999, request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Car not found."));
        _mockCarRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Car>()), Times.Never);
    }

    [TestCase("Chennai", 2)]
    [TestCase("Bangalore", 1)]
    [TestCase("Delhi", 0)]
    public async Task SearchAsync_WithVariousLocations_ReturnsExpectedCounts(string location, int expectedCount)
    {
        var searchRequest = new CarSearchRequestDto
        {
            Location = location,
            Page = 1,
            PageSize = 10
        };

        var allMockCars = new List<Car>
        {
            new Car { Id = 1, Location = "Chennai Airport", Make = "Toyota", Brand = new Brand() },
            new Car { Id = 2, Location = "Chennai Central", Make = "Honda", Brand = new Brand() },
            new Car { Id = 3, Location = "Bangalore Airport", Make = "Hyundai", Brand = new Brand() }
        };

        var filteredCars = allMockCars.Where(c => c.Location.Contains(location)).ToList();

        _mockCarRepository.Setup(repo => repo.SearchAsync(location, null, null, null, null, null, null, null))
                          .ReturnsAsync(filteredCars);

        var result = await _carService.SearchAsync(searchRequest);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(expectedCount));
        Assert.That(result.TotalCount, Is.EqualTo(expectedCount));
    }

    [Test]
    public async Task DeleteAsync_WhenCarExists_RemovesFromDatabase()
    {
        var carId = 1;
        var existingCar = new Car { Id = carId, Make = "Mahindra" };

        _mockCarRepository.Setup(repo => repo.GetByIdAsync(carId))
                          .ReturnsAsync(existingCar);

        var result = await _carService.DeleteAsync(carId);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("Request successful.")); 
        
        _mockCarRepository.Verify(repo => repo.DeleteAsync(existingCar), Times.Once);
        _mockCarRepository.Verify(repo => repo.SaveAsync(), Times.Once);
    }
}