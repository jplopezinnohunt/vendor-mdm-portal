using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<User>> Create([FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdUser = await _service.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
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
        // 1. Get Email from Claims
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        // 2. Fetch User
        var user = await _service.GetUserByEmailAsync(email);
        if (user == null) return NotFound();

        return Ok(user);
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
