using ProjectSaas.Api.Common.Extensions;
using ProjectSaas.Api.Common.Middleware;
using ProjectSaas.Api.Infrastructure.Auth;
using ProjectSaas.Api.Infrastructure.Security;
using ProjectSaas.Api.Application.Abstractions.Security;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using ProjectSaas.Api.Application.Security;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ProjectSaas.Api.Application.Notifications;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddControllers();

var postgresConnectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' was not found.");

builder.Services
    .AddHealthChecks()
    .AddNpgSql(
        connectionString: postgresConnectionString,
        name: "postgres",
        tags: new[] { "ready" });

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services
    .AddOptions<RefreshCookieOptions>()
    .Bind(builder.Configuration.GetSection(RefreshCookieOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Name),
        "Refresh cookie name is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Path) && options.Path.StartsWith('/'),
        "Refresh cookie path must start with '/'.")
    .Validate(options => options.SameSite is "Lax" or "Strict" or "None",
        "Refresh cookie SameSite must be Lax, Strict, or None.")
    .ValidateOnStart();

builder.Services.AddEndpointsApiExplorer();

// Swagger configuration with JWT Bearer support for testing secured endpoints
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, token) =>
    {
        var httpContext = context.HttpContext;

        var path = httpContext.Request.Path.Value ?? string.Empty;
        var method = httpContext.Request.Method;
        var ip = httpContext.Connection.RemoteIpAddress?.ToString();
        var occurredAtUtc = DateTimeOffset.UtcNow;

        string? eventType = path switch
        {
            "/api/auth/login" => SecurityEventTypes.LoginRateLimitExceeded,
            "/api/auth/refresh" => SecurityEventTypes.RefreshRateLimitExceeded,
            _ => null
        };

        if (eventType is not null)
        {
            var securityEventService =
                httpContext.RequestServices.GetRequiredService<ISecurityEventService>();

            var metadataJson = JsonSerializer.Serialize(new
            {
                path,
                method
            });

            await securityEventService.WriteAsync(
                eventType: eventType,
                userId: null,
                organisationId: null,
                familyId: null,
                requestIpAddress: ip,
                occurredAtUtc: occurredAtUtc,
                metadataJson: metadataJson,
                ct: token);
        }

        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsync(
            """
        {"title":"Too Many Requests","status":429,"detail":"Rate limit exceeded. Please try again later."}
        """,
            token);
    };

    options.AddPolicy("auth-login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("auth-refresh", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = jwt.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.SecretKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<ISecurityEventService, SecurityEventService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

app.UseRateLimiter();

app.UseCors("FrontendCors");

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapControllers();

app.Run();