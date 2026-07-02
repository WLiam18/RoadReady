using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadReady.CarService.Interfaces;
using RoadReady.Shared.DTOs.PromoCode;
using RoadReady.Shared.Enums;

namespace RoadReady.CarService.Controllers;

[ApiController]
[Route("api/v1/promo-codes")]
public class PromoCodesController : ControllerBase
{
    private readonly IPromoCodeService _service;

    public PromoCodesController(IPromoCodeService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Create([FromBody] CreatePromoCodeRequestDto request)
    {
        var result = await _service.CreateAsync(request);
        if (!result.Success) return BadRequest(result);
        return StatusCode(201, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Update(int id, [FromBody] CreatePromoCodeRequestDto request)
    {
        var result = await _service.UpdateAsync(id, request);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("validate")]
    [Authorize]
    public async Task<IActionResult> Validate([FromBody] ValidatePromoRequestDto request)
    {
        var result = await _service.ValidateAsync(request);
        return Ok(result);
    }
}
