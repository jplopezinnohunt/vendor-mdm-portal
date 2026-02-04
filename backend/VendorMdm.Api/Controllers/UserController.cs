using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<User>> Create([FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.CreateUserAsync(user);

        if (result.IsFailure)
        {
            // Check if it's a conflict (duplicate) error
            if (result.Error.Contains("already exists"))
            {
                return Conflict(new { message = result.Error });
            }
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<User>> UpdateRoles(Guid id, [FromBody] RoleUpdateRequest request)
    {
        try 
        {
            var user = await _service.UpdateUserRolesAsync(id, request.Roles, request.AuthMethod);
            if (user == null) return NotFound();
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user role");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<User>> Me()
    {
        try
        {
            // 1. Get Email from Claims
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            // 2. Fetch User
            var user = await _service.GetUserByEmailAsync(email);
            if (user == null) return NotFound();

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch current user profile");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message }); 
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<User>>> GetAll()
    {
        return Ok(await _service.GetAllUsersAsync());
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<User>> GetById(Guid id)
    {
        var user = await _service.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPut("{id}/block")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<User>> UpdateBlockStatus(Guid id, [FromBody] BlockStatusRequest request)
    {
        try 
        {
            var user = await _service.ToggleBlockStatusAsync(id, request.IsBlocked);
            if (user == null) return NotFound();
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update block status");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

public class RoleUpdateRequest 
{
    public List<string> Roles { get; set; } = new();
    public string? AuthMethod { get; set; }
}

public class BlockStatusRequest
{
    public bool IsBlocked { get; set; }
}
