using UserManagementApi.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// 1. Configure Services
// -----------------------------
builder.Services.AddControllers();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User Management API",
        Version = "v1",
        Description = "A simple API for managing users"
    });
});

builder.Services.AddSingleton<UserService>();

var app = builder.Build();

// -----------------------------
// 2. Configure Middleware
// -----------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        var error = exceptionHandlerFeature?.Error;

        logger.LogError(error, "Unhandled exception occurred");

        var errorResponse = new { error = "Internal server error." };
        var json = System.Text.Json.JsonSerializer.Serialize(errorResponse);

        await context.Response.WriteAsync(json);
    });
});

app.UseHttpLogging();
app.UseHttpsRedirection();

// 🔒 Custom token-based authentication
app.UseMiddleware<TokenAuthenticationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();

// -----------------------------
// 3. TokenAuthenticationMiddleware Class (inline for now)
// -----------------------------
public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenAuthenticationMiddleware> _logger;
    private const string AUTH_HEADER = "Authorization";
    private const string VALID_TOKEN = "Bearer my-secret-token"; // Replace with secure source in real apps

    public TokenAuthenticationMiddleware(RequestDelegate next, ILogger<TokenAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(AUTH_HEADER, out var token) || token != VALID_TOKEN)
        {
            _logger.LogWarning("Unauthorized request to {Path}", context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"Unauthorized. Valid token required.\"}");
            return;
        }

        await _next(context);
    }
}
