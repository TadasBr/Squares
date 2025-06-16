using System.Net;
using Microsoft.AspNetCore.Mvc;
using Squares.API.Dtos;
using Squares.Business.Exceptions;
using Squares.Business.Services;
using Squares.Persistence.Entities;

namespace Squares.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PointsController(IPointsService pointsService) : ControllerBase
{
    private readonly IPointsService _pointsService = pointsService;

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> AddPoint([FromBody] PointDto point)
    {
        try
        {
            await _pointsService.AddPointAsync(new Point(point.X, point.Y));
            return Ok($"Point ({point.X}, {point.Y}) added successfully.");
        }
        catch (DuplicatePointException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DeletePoint([FromBody] PointDto point)
    {
        try
        {
            await _pointsService.DeletePointAsync(point.X, point.Y);
            return Ok($"Point ({point.X}, {point.Y}) deleted successfully.");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("import")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ImportPoints([FromBody] string json)
    {
        try
        {
            await _pointsService.ImportPointsAsync(json);
            return Ok("Points imported successfully.");
        }
        catch (DuplicatePointException ex)
        {
            return BadRequest($"Import failed: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            return BadRequest($"Invalid input: {ex.Message}");
        }
    }

    [HttpGet("getsquares")]
    [ProducesResponseType(typeof(List<SquareDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetSquares()
    {
        var squares = await _pointsService.IdentifySquaresAsync();
        var result = squares.Select(s => new SquareDto(s)).ToList();
        return Ok(result);
    }
}
