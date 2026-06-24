using RoadReady.CarService.Interfaces;
using RoadReady.CarService.Models;
using RoadReady.Shared.DTOs.Car;
using RoadReady.Shared.Responses;

namespace RoadReady.CarService.Implementations;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly ILogger<BrandService> _logger;

    public BrandService(IBrandRepository brandRepository, ILogger<BrandService> logger)
    {
        _brandRepository = brandRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<List<BrandDto>>> GetAllAsync()
    {
        var brands = await _brandRepository.GetAllAsync();

        var brandDtos = brands.Select(MapBrandToDto).ToList();

        return ApiResponse<List<BrandDto>>.Ok(brandDtos, "Brands fetched successfully.");
    }

    public async Task<ApiResponse<BrandDto>> GetByIdAsync(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);

        if (brand == null)
        {
            return ApiResponse<BrandDto>.Fail("Brand not found.");
        }

        return ApiResponse<BrandDto>.Ok(MapBrandToDto(brand), "Brand fetched successfully.");
    }

    public async Task<ApiResponse<BrandDto>> CreateAsync(CreateBrandRequestDto request)
    {
        var existingBrand = await _brandRepository.GetByNameAsync(request.Name);

        if (existingBrand != null)
        {
            return ApiResponse<BrandDto>.Fail("Brand already exists.");
        }

        var brand = new Brand
        {
            Name = request.Name,
            LogoUrl = request.LogoUrl,
            Country = request.Country
        };

        await _brandRepository.AddAsync(brand);
        await _brandRepository.SaveAsync();

        _logger.LogInformation("Brand created successfully: {BrandName}", brand.Name);

        return ApiResponse<BrandDto>.Created(MapBrandToDto(brand), "Brand created successfully.");
    }

    public async Task<ApiResponse<BrandDto>> UpdateAsync(int id, CreateBrandRequestDto request)
    {
        var brand = await _brandRepository.GetByIdAsync(id);

        if (brand == null)
        {
            return ApiResponse<BrandDto>.Fail("Brand not found.");
        }

        brand.Name = request.Name;
        brand.LogoUrl = request.LogoUrl;
        brand.Country = request.Country;

        await _brandRepository.UpdateAsync(brand);
        await _brandRepository.SaveAsync();

        _logger.LogInformation("Brand updated successfully: {BrandId}", brand.Id);

        return ApiResponse<BrandDto>.Ok(MapBrandToDto(brand), "Brand updated successfully.");
    }

    public async Task<ApiResponse<string>> DeleteAsync(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);

        if (brand == null)
        {
            return ApiResponse<string>.Fail("Brand not found.");
        }

        await _brandRepository.DeleteAsync(brand);
        await _brandRepository.SaveAsync();

        _logger.LogInformation("Brand deleted successfully: {BrandId}", brand.Id);

        return ApiResponse<string>.Ok("Brand deleted successfully.");
    }

    private static BrandDto MapBrandToDto(Brand brand)
    {
        return new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            LogoUrl = brand.LogoUrl,
            Country = brand.Country
        };
    }
}
