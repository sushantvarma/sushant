using Microsoft.AspNetCore.Mvc;
using ReferenceDataService.Models;
using ReferenceDataService.Services;
using System.Diagnostics;

namespace ReferenceDataService.Controllers;

/// <summary>
/// API controller for reference data endpoints
/// </summary>
[ApiController]
[Route("api")]
public class ReferenceDataController : ControllerBase
{
    private readonly ISnowflakeService _snowflakeService;
    private readonly ILogger<ReferenceDataController> _logger;

    public ReferenceDataController(
        ISnowflakeService snowflakeService,
        ILogger<ReferenceDataController> logger)
    {
        _snowflakeService = snowflakeService;
        _logger = logger;
    }

    /// <summary>
    /// Get reference data from Snowflake
    /// Measures total execution time and logs request details with correlation ID
    /// </summary>
    /// <returns>Reference data with execution time in milliseconds</returns>
    [HttpGet("reference-data")]
    [ProducesResponseType(typeof(ReferenceDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status408RequestTimeout)]
    public async Task<IActionResult> GetReferenceData()
    {
        var stopwatch = Stopwatch.StartNew();
        var requestReceivedTime = DateTime.UtcNow;

        _logger.LogInformation(
            "Reference data request received - RequestTime: {RequestTime}, RemoteIp: {RemoteIp}",
            requestReceivedTime,
            HttpContext.Connection.RemoteIpAddress);

        try
        {
            _logger.LogInformation("Initiating Snowflake API call");

            // Call the Snowflake service to get raw JSON data
            var jsonData = await _snowflakeService.GetReferenceDataAsync();

            stopwatch.Stop();
            var totalExecutionMs = stopwatch.ElapsedMilliseconds;
            var responseTime = DateTime.UtcNow;

            _logger.LogInformation(
                "Snowflake API call completed successfully - ResponseTime: {ResponseTime}, ExecutionTimeMs: {ExecutionTimeMs}",
                responseTime,
                totalExecutionMs);

            // Parse JSON string to object for response
            var data = System.Text.Json.JsonSerializer.Deserialize<object>(jsonData);

            var response = new ReferenceDataResponse
            {
                Data = data,
                ExecutionTimeMs = totalExecutionMs
            };

            _logger.LogInformation(
                "Reference data endpoint completed - Status: 200 OK, TotalExecutionMs: {ExecutionTimeMs}",
                totalExecutionMs);

            return Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        {
            stopwatch.Stop();
            var totalExecutionMs = stopwatch.ElapsedMilliseconds;

            var errorResponse = new ErrorResponse
            {
                Error = "Request timeout while calling Snowflake API",
                ExecutionTimeMs = totalExecutionMs
            };

            _logger.LogWarning(
                "Snowflake API timeout - Error: {ErrorMessage}, ExecutionTimeMs: {ExecutionTimeMs}",
                ex.Message,
                totalExecutionMs);

            return StatusCode(StatusCodes.Status408RequestTimeout, errorResponse);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("HTTP request failed", StringComparison.OrdinalIgnoreCase))
        {
            stopwatch.Stop();
            var totalExecutionMs = stopwatch.ElapsedMilliseconds;

            var errorResponse = new ErrorResponse
            {
                Error = "Failed to connect to Snowflake API",
                ExecutionTimeMs = totalExecutionMs
            };

            _logger.LogWarning(
                "Snowflake API connection error - Error: {ErrorMessage}, ExecutionTimeMs: {ExecutionTimeMs}",
                ex.Message,
                totalExecutionMs);

            return StatusCode(StatusCodes.Status503ServiceUnavailable, errorResponse);
        }
        catch (InvalidOperationException ex)
        {
            stopwatch.Stop();
            var totalExecutionMs = stopwatch.ElapsedMilliseconds;

            var errorResponse = new ErrorResponse
            {
                Error = ex.Message,
                ExecutionTimeMs = totalExecutionMs
            };

            _logger.LogWarning(
                "Snowflake API error - Error: {ErrorMessage}, ExecutionTimeMs: {ExecutionTimeMs}",
                ex.Message,
                totalExecutionMs);

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var totalExecutionMs = stopwatch.ElapsedMilliseconds;

            var errorResponse = new ErrorResponse
            {
                Error = $"Unexpected error: {ex.Message}",
                ExecutionTimeMs = totalExecutionMs
            };

            _logger.LogError(ex,
                "Unexpected error in reference data endpoint - Error: {ErrorMessage}, ExecutionTimeMs: {ExecutionTimeMs}",
                ex.Message,
                totalExecutionMs);

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}
