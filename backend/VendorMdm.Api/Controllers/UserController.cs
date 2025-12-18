using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService service, ILogger<UserController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<User>> Create([FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Controller Layer: Correlation ID (handled by middleware usually, but can be explicit)
            // Call Service (Logic + Persistence + Events)
            var createdUser = await _service.CreateUserAsync(user);
            
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(Guid id)
    {
        var user = await _service.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }
}
