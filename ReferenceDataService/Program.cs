using Serilog;
using Serilog.Formatting.Compact;
using ReferenceDataService.Services;
using ReferenceDataService.Middleware;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Configure Serilog with structured JSON logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console(new CompactJsonFormatter())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ReferenceDataService")
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Reference Data Service", Version = "v1" });
});

// Register HttpClientFactory for creating HttpClient instances
builder.Services.AddHttpClient();

// Register in-memory caching service
builder.Services.AddMemoryCache();

// Register Snowflake service with dependency injection
builder.Services.AddScoped<ISnowflakeService, SnowflakeService>();

// Add correlation ID middleware
builder.Services.AddScoped<CorrelationIdMiddleware>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add correlation ID middleware to the pipeline
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information("Starting ReferenceDataService application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
