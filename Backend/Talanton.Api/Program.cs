using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;
using Talanton.Api.Repositories;
using Talanton.Api.Repositories.Interfaces;
using Talanton.Api.Services;
using Talanton.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Allowed browser origins come from configuration so that a new frontend deployment
// (a Vercel preview URL, a renamed project) does not require a code change.
//   Exact origins:  Cors:AllowedOrigins        or env CORS_ALLOWED_ORIGINS
//   Wildcards:      Cors:AllowedOriginPatterns or env CORS_ALLOWED_ORIGIN_PATTERNS
// Both accept a comma-separated string; the defaults below apply when neither is set.
var allowedOrigins = ReadOriginList(builder.Configuration, "Cors:AllowedOrigins", "CORS_ALLOWED_ORIGINS", DefaultAllowedOrigins());
var allowedOriginPatterns = ReadOriginList(builder.Configuration, "Cors:AllowedOriginPatterns", "CORS_ALLOWED_ORIGIN_PATTERNS", DefaultAllowedOriginPatterns());

Console.WriteLine($"[DEBUG] CORS allowed origins: {string.Join(", ", allowedOrigins)}");
Console.WriteLine($"[DEBUG] CORS allowed origin patterns: {string.Join(", ", allowedOriginPatterns)}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => IsOriginAllowed(origin, allowedOrigins, allowedOriginPatterns))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString = FirstNonEmpty(
    builder.Configuration["SUPABASE_DB_CONNECTION"],
    builder.Configuration["DATABASE_URL"],
    builder.Configuration.GetConnectionString("DefaultConnection"));

var source = "none";
if (!string.IsNullOrWhiteSpace(builder.Configuration["SUPABASE_DB_CONNECTION"]))
{
    source = "SUPABASE_DB_CONNECTION";
}
else if (!string.IsNullOrWhiteSpace(builder.Configuration["DATABASE_URL"]))
{
    source = "DATABASE_URL";
}
else if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
{
    source = "ConnectionStrings:DefaultConnection";
}

Console.WriteLine($"[DEBUG] connectionString present: {!string.IsNullOrWhiteSpace(connectionString)}");
Console.WriteLine($"[DEBUG] connectionString source: {source}");

if (string.IsNullOrWhiteSpace(connectionString) || HasPlaceholderConnectionString(connectionString))
{
    throw new InvalidOperationException(
        "No valid PostgreSQL connection string configured. Set SUPABASE_DB_CONNECTION or ConnectionStrings:DefaultConnection.");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.EnableRetryOnFailure()));

builder.Services.AddScoped<IApplicantRepository, ApplicantRepository>();
builder.Services.AddScoped<IApplicantService, ApplicantService>();
builder.Services.AddScoped<ILoanApplicationService, LoanApplicationService>();

var app = builder.Build();

// Startup database check. Each step reports its own outcome: a service that cannot reach
// its database still answers on the endpoints backed by in-memory data, which previously
// made a completely disconnected database look like a healthy deployment.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var reachable = false;
    try
    {
        reachable = await db.Database.CanConnectAsync();
        Console.WriteLine(reachable
            ? "[STARTUP] Database connection: OK"
            : "[ERROR] Database connection: UNREACHABLE. Every database-backed endpoint will fail. " +
              "Check SUPABASE_DB_CONNECTION — it must be in Npgsql key-value form " +
              "(Host=...;Port=...;Database=...;Username=...;Password=...;SSL Mode=Require), not a postgres:// URI.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Database connection check threw {ex.GetType().Name}: {ex.Message}");
    }

    if (reachable)
    {
        try
        {
            var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
            if (pending.Count == 0)
            {
                Console.WriteLine("[STARTUP] Migrations: up to date");
            }
            else
            {
                Console.WriteLine($"[STARTUP] Migrations: applying {pending.Count} pending ({string.Join(", ", pending)})");
                await db.Database.MigrateAsync();
                Console.WriteLine("[STARTUP] Migrations: applied");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Migration failed ({ex.GetType().Name}): {ex.Message}. " +
                              "Tables added by unapplied migrations will return 500 until this is resolved.");
        }

        try
        {
            await DemoUsersSeeder.SeedAsync(db);
            Console.WriteLine("[STARTUP] Demo user seeding: OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Demo user seeding failed ({ex.GetType().Name}): {ex.Message}. " +
                              "Login will reject valid demo credentials.");
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        Application = "Talanton Trust Engine API",
        Status = "Running",
        Environment = app.Environment.EnvironmentName,
        Timestamp = DateTime.UtcNow
    });
});

app.Run();

static bool HasPlaceholderConnectionString(string value)
{
    return value.Contains("YOUR_SUPABASE_HOST", StringComparison.OrdinalIgnoreCase)
        || value.Contains("__SET_IN_ENV__", StringComparison.OrdinalIgnoreCase)
        || value.Contains("__SET_IN_ENV_OR_USE_SUPABASE_DB_CONNECTION__", StringComparison.OrdinalIgnoreCase);
}

static string? FirstNonEmpty(params string?[] values)
{
    foreach (var value in values)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
    }

    return null;
}

static string[] DefaultAllowedOrigins() =>
[
    "http://localhost:5173",
    "http://localhost:3000",
    "http://localhost:3001",
    "https://talanton-trust-engine.vercel.app",
    "https://talanton-trust-engine-7.onrender.com"
];

// Vercel gives every preview deployment its own hostname, so the project's preview
// range is matched by pattern rather than listed origin by origin.
static string[] DefaultAllowedOriginPatterns() =>
[
    "https://talanton-trust-engine-*-amke0501s-projects.vercel.app"
];

/// <summary>
/// Reads a list of origins from an appsettings array, a colon-key, or an environment
/// variable. Comma- and semicolon-separated values are accepted so the list can be set
/// as a single environment variable on hosts such as Render.
/// </summary>
static string[] ReadOriginList(IConfiguration configuration, string sectionKey, string environmentKey, string[] fallback)
{
    var fromSection = configuration.GetSection(sectionKey).Get<string[]>();
    if (fromSection is { Length: > 0 })
    {
        return Split(fromSection);
    }

    var raw = FirstNonEmpty(configuration[sectionKey], configuration[environmentKey]);
    if (!string.IsNullOrWhiteSpace(raw))
    {
        return Split([raw]);
    }

    return fallback;

    static string[] Split(string[] values) => values
        .SelectMany(value => value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        .Select(value => value.TrimEnd('/'))
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

/// <summary>
/// Matches a request origin against the exact allow-list first, then the wildcard
/// patterns. A "*" in a pattern matches any run of characters except "/", so it cannot
/// widen a match past the hostname.
/// </summary>
static bool IsOriginAllowed(string origin, string[] allowedOrigins, string[] allowedPatterns)
{
    if (string.IsNullOrWhiteSpace(origin))
    {
        return false;
    }

    var candidate = origin.TrimEnd('/');

    if (allowedOrigins.Contains(candidate, StringComparer.OrdinalIgnoreCase))
    {
        return true;
    }

    foreach (var pattern in allowedPatterns)
    {
        if (pattern == "*")
        {
            return true;
        }

        var expression = "^" + string.Join(
            "[^/]*",
            pattern.Split('*').Select(System.Text.RegularExpressions.Regex.Escape)) + "$";

        if (System.Text.RegularExpressions.Regex.IsMatch(candidate, expression, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            return true;
        }
    }

    return false;
}

