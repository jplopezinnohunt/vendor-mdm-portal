using Microsoft.AspNetCore.Mvc;

namespace VendorMdm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorController : ControllerBase
{
    // In a real app, this would inject an ISapService
    public VendorController()
    {
    }

    [HttpGet("{id}")]
    public IActionResult GetVendor(string id)
    {
        // Mock SAP Data
        var sapData = new
        {
            VendorId = id,
            Name = "Acme Corp",
            Address = "123 SAP Street",
            Source = "SAP D01"
        };

        // TODO: Overlay Deltas from Repository if needed (as per requirements)
        // For now returning SAP data directly.

        return Ok(sapData);
    }
}
