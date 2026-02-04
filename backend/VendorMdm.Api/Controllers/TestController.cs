using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace VendorMdm.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly SqlDbContext _context;
    private readonly IWebHostEnvironment _env;

    public TestController(SqlDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost("seed-application")]
    public async Task<IActionResult> SeedApplication([FromBody] SeedApplicationRequest request)
    {
        if (!_env.IsDevelopment())
        {
            return NotFound();
        }

        var app = new VendorApplication
        {
            Id = Guid.NewGuid(),
            CompanyName = request.CompanyName ?? "Test Vendor Seeded",
            TaxId = request.TaxId ?? "TEST-TAX-ID",
            ContactName = request.ContactName ?? "Test User",
            ContactEmail = request.ContactEmail ?? "test.seeded@example.com",
            Status = "PendingReview",
            RegistrationType = "Direct",
            CreatedAt = DateTime.UtcNow,
            Attributes = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "VendorType", "Company" },
                { "AccountGroup", "HQSU" },
                { "country", "US" },
                { "currency", "USD" }
            })
        };

        _context.VendorApplications.Add(app);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Seeded Application Created", applicationId = app.Id });
    }

    public class SeedApplicationRequest
    {
        public string? CompanyName { get; set; }
        public string? TaxId { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
    }
}
