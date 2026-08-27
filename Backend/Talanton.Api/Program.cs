using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;
using Talanton.Api.Repositories;
using Talanton.Api.Repositories.Interfaces;
using Talanton.Api.Services;
using Talanton.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:3000","http://localhost:3001","https://talanton-trust-engine.vercel.app",
"https://talanton-trust-engine-7.onrender.com")
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await DemoUsersSeeder.SeedAsync(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARNING] Database seeding warning: {ex.Message}");
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

