using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models.Sanctions;

namespace VendorMdm.Api.Controllers;

/// <summary>
/// Sanctions screening API - Check entities against global sanctions lists
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sanctions")]
[Route("api/sanctions")]
[Produces("application/json")]
public class SanctionsController : ControllerBase
{
    private readonly ISanctionsScreeningService _sanctionsService;
    private readonly ILogger<SanctionsController> _logger;

    public SanctionsController(
        ISanctionsScreeningService sanctionsService,
        ILogger<SanctionsController> logger)
    {
        _sanctionsService = sanctionsService;
        _logger = logger;
    }

    /// <summary>
    /// Screen an entity against all sanctions lists
    /// </summary>
    /// <param name="request">Entity details to screen</param>
    /// <returns>Screening result with matches and risk assessment</returns>
    [HttpPost("screen")]
    [ProducesResponseType(typeof(ScreeningResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ScreeningResult>> ScreenEntity([FromBody] ScreeningRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.EntityName))
            {
                return BadRequest(new { error = "EntityName is required" });
            }

            if (string.IsNullOrWhiteSpace(request.VendorId))
            {
                return BadRequest(new { error = "VendorId is required" });
            }

            var result = await _sanctionsService.ScreenEntityAsync(request);
            return Ok(result);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                error = "Real sanctions screening service not configured",
                details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sanctions screening");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An error occurred during sanctions screening",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Batch screen multiple entities
    /// </summary>
    /// <param name="requests">List of entities to screen</param>
    /// <returns>List of screening results</returns>
    [HttpPost("screen/batch")]
    [ProducesResponseType(typeof(List<ScreeningResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ScreeningResult>>> ScreenBatch([FromBody] List<ScreeningRequest> requests)
    {
        try
        {
            if (requests == null || !requests.Any())
            {
                return BadRequest(new { error = "At least one screening request is required" });
            }

            var results = await _sanctionsService.ScreenBatchAsync(requests);
            return Ok(results);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                error = "Real sanctions screening service not configured",
                details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during batch sanctions screening");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An error occurred during batch screening",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Get screening result by ID
    /// </summary>
    /// <param name="screeningId">Screening ID</param>
    /// <returns>Screening result</returns>
    [HttpGet("{screeningId}")]
    [ProducesResponseType(typeof(ScreeningResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScreeningResult>> GetScreeningResult(string screeningId)
    {
        try
        {
            var result = await _sanctionsService.GetScreeningResultAsync(screeningId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Screening result {screeningId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving screening result");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An error occurred retrieving the screening result",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Get information about when sanctions lists were last updated
    /// </summary>
    /// <returns>Lists update information</returns>
    [HttpGet("lists/info")]
    [ProducesResponseType(typeof(ListsUpdateInfo), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListsUpdateInfo>> GetListsUpdateInfo()
    {
        try
        {
            var info = await _sanctionsService.GetListsUpdateInfoAsync();
            return Ok(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lists update info");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An error occurred retrieving list information",
                details = ex.Message
            });
        }
    }
}
