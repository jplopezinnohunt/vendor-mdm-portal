using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FundController : ControllerBase
{
    private readonly IFundService _service;

    public FundController(IFundService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Fund>>> GetFunds()
    {
        return await _service.GetAllFundsAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Fund>> GetFund(Guid id)
    {
        var fund = await _service.GetFundByIdAsync(id);

        if (fund == null)
        {
            return NotFound();
        }

        return fund;
    }

    [HttpPost]
    public async Task<ActionResult<Fund>> CreateFund([FromBody] Fund fund)
    {
        try
        {
            var created = await _service.CreateFundAsync(fund);
            return CreatedAtAction(nameof(GetFund), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFund(Guid id, Fund fund)
    {
        if (id != fund.Id)
        {
            return BadRequest();
        }

        try
        {
            await _service.UpdateFundAsync(fund);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (await _service.GetFundByIdAsync(id) == null)
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }
}
