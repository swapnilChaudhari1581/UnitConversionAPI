using UnitConversionAPI.Data;
using UnitConversionAPI.Middleware;
using UnitConversionAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers & JSON ───────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── OpenAPI / Swagger ────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── Domain services ──────────────────────────────────────────────────────────
// UnitRegistry is a singleton: it is immutable after construction and shared
// across all requests to avoid redundant dictionary allocations.
builder.Services.AddSingleton<IUnitRegistry, UnitRegistry>();
builder.Services.AddScoped<IConversionService, ConversionService>();

// ── Problem Details (RFC 7807) ───────────────────────────────────────────────
builder.Services.AddProblemDetails();

// ── CORS (relaxed for local development; tighten per environment in prod) ────
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// ============================================================================
var app = builder.Build();
// ============================================================================

// Global exception handler must be first in the pipeline so it wraps
// every subsequent middleware including routing and controller execution.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    // Serves the OpenAPI JSON at /openapi/v1.json
    app.MapOpenApi();
    app.UseCors();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Required so that WebApplicationFactory<Program> in the test project can
// reference the entry-point type without making the whole class public.
public partial class Program { }
