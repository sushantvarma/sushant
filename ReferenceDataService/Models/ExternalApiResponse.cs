namespace ReferenceDataService.Models;

/// <summary>
/// Represents the external API response from Snowflake
/// </summary>
public class ExternalApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
}

/// <summary>
/// Represents the API response returned to the client with timing information
/// </summary>
public class ReferenceDataResponse
{
    public object? Data { get; set; }
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// Represents an error response
/// </summary>
public class ErrorResponse
{
    public string? Error { get; set; }
    public long ExecutionTimeMs { get; set; }
}
