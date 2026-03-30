using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace ReferenceDataService.Services;

/// <summary>
/// Implementation of Snowflake API service
/// Sends requests to external Snowflake API endpoint with logging and error handling
/// Includes in-memory caching for improved performance
/// </summary>
public class SnowflakeService : ISnowflakeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SnowflakeService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;

    // Cache key for reference data
    private const string CacheKey = "reference-data";
    
    // Cache duration: 15 minutes
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    // Placeholder for Snowflake API Authorization token
    private const string AuthorizationTokenPlaceholder = "Bearer YOUR_SNOWFLAKE_AUTH_TOKEN_HERE";

    public SnowflakeService(
        IHttpClientFactory httpClientFactory,
        ILogger<SnowflakeService> logger,
        IConfiguration configuration,
        IMemoryCache memoryCache)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
        _memoryCache = memoryCache;
    }

    public async Task<string> GetReferenceDataAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        var requestStartTime = DateTime.UtcNow;
        var snowflakeBaseUrl = _configuration["ExternalApi:SnowflakeBaseUrl"] ?? "https://snowflake.com";
        var timeoutSeconds = int.Parse(_configuration["ExternalApi:TimeoutSeconds"] ?? "30");

        // Check if data exists in cache
        if (_memoryCache.TryGetValue(CacheKey, out string? cachedData))
        {
            stopwatch.Stop();
            
            _logger.LogInformation(
                "Reference data retrieved from memory cache - CacheKey: {CacheKey}, CacheDuration: {CacheDuration}, SourceType: Cache",
                CacheKey,
                CacheDuration.TotalMinutes);

            return cachedData!;
        }

        _logger.LogInformation(
            "Reference data not found in cache, fetching from Snowflake API - BaseUrl: {BaseUrl}, Timeout: {TimeoutSeconds}s, SourceType: API, Timestamp: {StartTime}",
            snowflakeBaseUrl,
            timeoutSeconds,
            requestStartTime);

        try
        {
            // Create HttpClient instance using IHttpClientFactory
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

            // Build the request URI
            var requestUri = new Uri($"{snowflakeBaseUrl}/api/reference-data");

            // Create request with headers
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            
            // Add Authorization header (placeholder - replace with actual token)
            request.Headers.Add("Authorization", AuthorizationTokenPlaceholder);
            request.Headers.Add("Accept", "application/json");

            _logger.LogInformation(
                "Sending HTTP GET request to Snowflake API - Uri: {Uri}, Headers: Authorization (Bearer), Accept: application/json",
                requestUri);

            // Send the request
            var response = await httpClient.SendAsync(request);

            stopwatch.Stop();
            var endTime = DateTime.UtcNow;
            var durationMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Snowflake API response received - StatusCode: {StatusCode}, Timestamp: {EndTime}, DurationMs: {DurationMs}",
                response.StatusCode,
                endTime,
                durationMs);

            // Handle non-200 responses
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogError(
                    "Snowflake API returned error - StatusCode: {StatusCode}, Content: {ErrorContent}, DurationMs: {DurationMs}",
                    response.StatusCode,
                    errorContent,
                    durationMs);

                throw new HttpRequestException($"API returned status code {response.StatusCode}: {errorContent}");
            }

            // Read and return raw JSON response
            var jsonResponse = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "Successfully retrieved reference data from Snowflake API - ResponseSize: {ResponseSize} bytes, DurationMs: {DurationMs}",
                jsonResponse.Length,
                durationMs);

            // Store in cache with 15-minute expiration
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration);

            _memoryCache.Set(CacheKey, jsonResponse, cacheOptions);

            _logger.LogInformation(
                "Reference data cached in memory - CacheKey: {CacheKey}, CacheDuration: {CacheDuration} minutes, ExpiresAt: {ExpirationTime}",
                CacheKey,
                CacheDuration.TotalMinutes,
                DateTime.UtcNow.Add(CacheDuration));

            return jsonResponse;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            var durationMs = stopwatch.ElapsedMilliseconds;

            _logger.LogError(ex,
                "HTTP request error occurred calling Snowflake API - Error: {ErrorMessage}, DurationMs: {DurationMs}",
                ex.Message,
                durationMs);

            throw new InvalidOperationException($"Failed to retrieve data from Snowflake API: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            stopwatch.Stop();
            var durationMs = stopwatch.ElapsedMilliseconds;
            var timeoutMessage = $"Request timeout after {timeoutSeconds} seconds";

            _logger.LogError(ex,
                "Snowflake API request timeout - Message: {TimeoutMessage}, DurationMs: {DurationMs}",
                timeoutMessage,
                durationMs);

            throw new InvalidOperationException(timeoutMessage, ex);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var durationMs = stopwatch.ElapsedMilliseconds;

            _logger.LogError(ex,
                "Unexpected error occurred calling Snowflake API - Error: {ErrorMessage}, DurationMs: {DurationMs}",
                ex.Message,
                durationMs);

            throw new InvalidOperationException($"Unexpected error calling Snowflake API: {ex.Message}", ex);
        }
    }
}
