using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;
using Talanton.Api.Repositories;
using Talanton.Api.Repositories.Interfaces;
using Talanton.Api.Services;
using Talanton.Api.Services.Interfaces;

// Every figure this API puts in a sentence — "270,000,000 UGX", a ratio of "1.07" — is formatted
// here and read in Uganda. Left to the host's locale it followed whatever the server happened to
// be set to, so the same shortfall rendered as "1,07" and "270 000 000" on one machine and
// "1.07" and "270,000,000" on another. Pin it, so the words a committee reads do not depend on
// where the container runs.
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

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

// An explicitly requested throwaway database, for running the whole stack on a laptop with no
// PostgreSQL to hand. It has to be asked for by name — a missing connection string still fails
// loudly rather than quietly starting on a database that forgets everything on restart, which
// would be far worse in a deployment than not starting at all.
var useInMemoryDatabase = string.Equals(
    FirstNonEmpty(builder.Configuration["USE_INMEMORY_DB"], builder.Configuration["Database:UseInMemory"]),
    "true", StringComparison.OrdinalIgnoreCase);

if (!useInMemoryDatabase && (string.IsNullOrWhiteSpace(connectionString) || HasPlaceholderConnectionString(connectionString)))
{
    throw new InvalidOperationException(
        "No valid PostgreSQL connection string configured. Set SUPABASE_DB_CONNECTION or ConnectionStrings:DefaultConnection. " +
        "For local development without PostgreSQL, set USE_INMEMORY_DB=true — the data is discarded when the process exits.");
}

// Authentication is Supabase's. The API needs only the project URL: the signing keys are public
// and fetched from it, so there is no secret here to leak or to keep in step with a rotation.
//
// A missing URL is a hard failure for the same reason a missing connection string is. An API that
// starts without authentication is not a degraded API — it is an open one, and it would look
// perfectly healthy while every guardrail in this service could be bypassed by anyone who knew
// the address. The local-development path below is opt-in by name and throws its data away.
var supabaseUrl = FirstNonEmpty(
    builder.Configuration["SUPABASE_URL"],
    builder.Configuration["NEXT_PUBLIC_SUPABASE_URL"])?.TrimEnd('/');

if (string.IsNullOrWhiteSpace(supabaseUrl) && !useInMemoryDatabase)
{
    throw new InvalidOperationException(
        "SUPABASE_URL is not configured, so logins cannot be verified and every endpoint would be " +
        "open. Set it to your Supabase project URL (https://<project>.supabase.co). For local " +
        "development without Supabase, set USE_INMEMORY_DB=true, which also enables development " +
        "tokens against a throwaway database.");
}

Console.WriteLine($"[DEBUG] Supabase auth authority: {supabaseUrl ?? "(none — local development tokens only)"}/auth/v1");

builder.Services.AddHttpClient();
builder.Services.AddScoped<SupabaseAdminService>();
builder.Services.AddTalantonAuthentication(supabaseUrl, allowLocalDevTokens: useInMemoryDatabase);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

if (useInMemoryDatabase)
{
    Console.WriteLine("[STARTUP] USE_INMEMORY_DB=true — running on a throwaway in-memory database. " +
                      "Nothing written here survives the process.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("talanton-local"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.EnableRetryOnFailure()));
}

builder.Services.AddScoped<IApplicantRepository, ApplicantRepository>();
builder.Services.AddScoped<IApplicantService, ApplicantService>();
builder.Services.AddScoped<ILoanApplicationService, LoanApplicationService>();
builder.Services.AddScoped<LiquidityService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<NotificationService>();

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
        if (useInMemoryDatabase)
        {
            // Nothing to connect to — the provider is the process itself.
            reachable = true;
        }
        else
        {
            // Open the connection rather than calling CanConnectAsync(): that swallows the
            // underlying exception and returns a bare false, which says nothing about whether
            // the string is malformed, the host is unreachable, or TLS failed.
            await db.Database.OpenConnectionAsync();
            await db.Database.CloseConnectionAsync();
            Console.WriteLine("[STARTUP] Database connection: OK");
            reachable = true;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Database connection: UNREACHABLE ({ex.GetType().Name}): {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"[ERROR]   caused by ({ex.InnerException.GetType().Name}): {ex.InnerException.Message}");
        }
        Console.WriteLine("[ERROR] Every database-backed endpoint will fail. Check SUPABASE_DB_CONNECTION:");
        Console.WriteLine("[ERROR]   - it must be Npgsql key-value form, not a postgres:// URI");
        Console.WriteLine("[ERROR]   - Supabase's pooler presents a self-signed certificate, so it needs " +
                          "'SSL Mode=Require;Trust Server Certificate=true' — without the trust flag Npgsql " +
                          "rejects the chain and the failure looks identical to an unreachable host");
    }

    if (reachable && useInMemoryDatabase)
    {
        // The in-memory provider has no migration history to apply; the schema comes straight
        // from the model.
        await db.Database.EnsureCreatedAsync();
        Console.WriteLine("[STARTUP] In-memory schema created from the model");
        await LocalDemoSeeder.SeedAsync(db);
    }
    else if (reachable)
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

// Who are you, then what may you do. Both must come before the endpoints they protect.
app.UseAuthentication();
app.UseAuthorization();

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
}).AllowAnonymous();

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
    "https://talanton-trust-engine-benis-roln.vercel.app",
    "https://talanton-trust-engine-9.onrender.com"
];

// Vercel gives every deployment its own hostname — one per project, plus one per preview —
// so the ranges are matched by pattern rather than listed origin by origin. Twice now a
// working frontend has been blocked purely because its hostname was not in a hardcoded list,
// which is silent from the server's side and looks like an unreachable API from the browser's.
//
// Deliberately not "https://talanton-trust-engine-*.vercel.app": that would admit any
// vercel.app subdomain starting with the project name, and this API has no authentication.
static string[] DefaultAllowedOriginPatterns() =>
[
    "https://talanton-trust-engine-benis-*.vercel.app",
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

