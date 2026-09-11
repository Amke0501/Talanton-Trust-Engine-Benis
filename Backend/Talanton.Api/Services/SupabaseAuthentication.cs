using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Talanton.Api.Services;

/// <summary>
/// Verifies that a request carries a genuine Supabase login.
///
/// Supabase signs its tokens with rotating asymmetric keys and publishes the public half at a
/// well-known address, so this API holds no secret of its own: it fetches the keys, verifies the
/// signature, and re-fetches when they rotate. Nothing to leak, nothing to keep in step.
///
/// The policy names below are the three portals. Authorisation inside a portal — which committee
/// seat may release funds, for instance — is a question about the SACCO's own records, and is
/// answered by <see cref="CurrentUserService"/> rather than by anything in the token.
/// </summary>
public static class SupabaseAuthentication
{
    public const string ApplicantPolicy = "portal:applicant";
    public const string UnderwriterPolicy = "portal:underwriter";
    public const string CommitteePolicy = "portal:committee";

    /// <summary>Every Supabase access token carries this audience.</summary>
    private const string SupabaseAudience = "authenticated";

    /// <summary>
    /// The scheme used when running against the throwaway local database, so the stack can be
    /// exercised without reaching a real Supabase project. It exists only when USE_INMEMORY_DB is
    /// set, which a deployment cannot be: see Program.cs, where a missing connection string is
    /// still a hard failure.
    /// </summary>
    public const string LocalDevScheme = "LocalDev";

    public static void AddTalantonAuthentication(
        this IServiceCollection services, string? supabaseUrl, bool allowLocalDevTokens)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<CurrentUserService>();

        var authority = string.IsNullOrWhiteSpace(supabaseUrl)
            ? null
            : $"{supabaseUrl.TrimEnd('/')}/auth/v1";

        // Only schemes that are actually registered may be named in a policy: naming an absent
        // one fails every request at runtime with "no authentication handler is registered",
        // which reads like a broken deployment rather than a configuration gap.
        var schemes = new List<string>();
        if (!string.IsNullOrWhiteSpace(supabaseUrl)) schemes.Add(JwtBearerDefaults.AuthenticationScheme);
        if (allowLocalDevTokens) schemes.Add(LocalDevScheme);

        if (schemes.Count == 0)
        {
            throw new InvalidOperationException(
                "No way to verify a login was configured. Set SUPABASE_URL, or USE_INMEMORY_DB=true " +
                "for local development.");
        }

        var builder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = schemes[0];
            options.DefaultChallengeScheme = schemes[0];
        });

        if (authority is not null)
        {
            builder.AddJwtBearer(options =>
            {
                // Authority drives OpenID discovery, which gives us the signing keys and keeps
                // them current on its own.
                options.Authority = authority;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authority,
                    ValidateAudience = true,
                    ValidAudience = SupabaseAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    // Supabase puts the address in "email"; ASP.NET looks for the long-form claim
                    // unless told otherwise, and without this every request resolves to nobody.
                    NameClaimType = "email",
                    RoleClaimType = "role",
                    ClockSkew = TimeSpan.FromSeconds(30),
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"[AUTH] Rejected a token ({context.Exception.GetType().Name}): " +
                                          $"{context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                };
            });
        }

        if (allowLocalDevTokens)
        {
            Console.WriteLine("[STARTUP] Local development tokens are ACCEPTED. This is only reachable " +
                              "with USE_INMEMORY_DB=true and must never be how a deployment runs.");

            builder.AddJwtBearer(LocalDevScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = LocalDevTokens.Issuer,
                    ValidateAudience = true,
                    ValidAudience = SupabaseAudience,
                    ValidateLifetime = true,
                    IssuerSigningKey = LocalDevTokens.SigningKey,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "email",
                    RoleClaimType = "role",
                };
            });
        }

        services.AddAuthorization(options =>
        {
            options.AddPolicy(ApplicantPolicy, p => p.RequireAssertion(_ => true));
            options.AddPolicy(UnderwriterPolicy, p => p.RequireAssertion(_ => true));
            options.AddPolicy(CommitteePolicy, p => p.RequireAssertion(_ => true));

            // Every endpoint requires a verified login unless it opts out with [AllowAnonymous].
            options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(
                    schemes.ToArray())
                .RequireAuthenticatedUser()
                .Build();
        });
    }
}

/// <summary>
/// Mints the short-lived tokens used by the local harness. The key is derived from a fixed
/// development string and is deliberately worthless: it is only ever trusted by a process started
/// with USE_INMEMORY_DB=true, which throws away its data on exit.
/// </summary>
public static class LocalDevTokens
{
    public const string Issuer = "talanton-local-dev";

    private const string DevSecret =
        "talanton-local-development-only-signing-key-not-a-secret-0123456789";

    public static readonly SymmetricSecurityKey SigningKey =
        new(Encoding.UTF8.GetBytes(DevSecret));

    public static string Issue(string email, TimeSpan lifetime)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = "authenticated",
            Expires = DateTime.UtcNow.Add(lifetime),
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim("email", email),
                new Claim("role", "authenticated"),
            }),
            SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256),
        };

        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
