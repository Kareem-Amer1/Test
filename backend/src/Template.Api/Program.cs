using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using VoiceFlowStudio.Api.Localization;
using VoiceFlowStudio.Api.Middleware;
using VoiceFlowStudio.Contracts.Common;
using VoiceFlowStudio.Infrastructure;
using VoiceFlowStudio.Infrastructure.Auth;
using VoiceFlowStudio.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// --- Infrastructure / DI ----------------------------------------------------
builder.Services.AddInfrastructure(config);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ILocalizer, JsonLocalizer>();

// --- AuthN / AuthZ (RS256 default; [Authorize] is the default fallback) -----
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = config.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
        SecurityKey signingKey;
        if (!string.IsNullOrWhiteSpace(jwt.PublicKeyPem))
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(jwt.PublicKeyPem);
            signingKey = new RsaSecurityKey(rsa);
        }
        else
        {
            // Dev fallback: parse private key for validation when only it is provided.
            var rsa = RSA.Create();
            if (!string.IsNullOrWhiteSpace(jwt.PrivateKeyPem)) rsa.ImportFromPem(jwt.PrivateKeyPem);
            signingKey = new RsaSecurityKey(rsa);
        }
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = signingKey,
            ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
            NameClaimType = "sub",
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Constitution IV: default = authenticated. Public endpoints opt out with [AllowAnonymous].
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// --- MVC --------------------------------------------------------------------
builder.Services
    .AddControllers(options => options.Filters.Add(new AuthorizeFilter()))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// --- Swagger / OpenAPI (Constitution III) -----------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VoiceFlow Studio API", Version = "v1" });
    var xml = Path.Combine(AppContext.BaseDirectory, "VoiceFlowStudio.Api.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>(),
    });
});

// --- CORS (frontend at localhost:8080 in dev) -------------------------------
var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

// --- Rate limiting (Constitution X) -----------------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var key = httpContext.User?.FindFirst("sub")?.Value
                  ?? httpContext.Connection.RemoteIpAddress?.ToString()
                  ?? "anon";
        var permit = config.GetValue<int?>("RateLimit:PermitLimit") ?? 100;
        var window = config.GetValue<int?>("RateLimit:WindowSeconds") ?? 60;
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permit,
            Window = TimeSpan.FromSeconds(window),
            QueueLimit = 0,
            AutoReplenishment = true,
        });
    });
});

// --- Outbound HTTP resilience (Constitution X) ------------------------------
// Global circuit breaker policy registered via IHttpClientFactory. Feature
// HTTP clients (e.g. AI Gateway) should be added with `.AddPolicyHandler(...)`.
builder.Services.AddHttpClient("default")
    .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
        .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 5, durationOfBreak: TimeSpan.FromSeconds(30)))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(15)));

var app = builder.Build();

// Ensure Mongo indexes at boot (single-document writes, idempotent).
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IMongoClientFactory>();
    try { await CollectionBootstrap.EnsureIndexesAsync(factory); }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Mongo index initialisation failed (continuing).");
    }
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "VoiceFlow Studio API v1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();

// Exposed for integration tests via WebApplicationFactory<T>.
public partial class Program;
