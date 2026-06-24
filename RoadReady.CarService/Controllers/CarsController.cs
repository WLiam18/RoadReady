using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadReady.CarService.Interfaces;
using RoadReady.Shared.DTOs.Car;
using RoadReady.Shared.Enums;

namespace RoadReady.CarService.Controllers;

[ApiController]
[Route("api/v1/cars")]
public class CarsController : ControllerBase
{
    private readonly ICarService _carService;

    public CarsController(ICarService carService)
    {
        _carService = carService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _carService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _carService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] CarSearchRequestDto request)
    {
        var result = await _carService.SearchAsync(request);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Create([FromBody] CreateCarRequestDto request)
    {
        var result = await _carService.CreateAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return StatusCode(201, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCarRequestDto request)
    {
        var result = await _carService.UpdateAsync(id, request);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _carService.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
