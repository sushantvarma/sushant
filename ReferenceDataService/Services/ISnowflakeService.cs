namespace ReferenceDataService.Services;

/// <summary>
/// Interface for Snowflake API service
/// </summary>
public interface ISnowflakeService
{
    /// <summary>
    /// Retrieves reference data from Snowflake API
    /// </summary>
    /// <returns>Raw JSON response string from the API</returns>
    Task<string> GetReferenceDataAsync();
}
