using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Talanton.Api.Services;

/// <summary>
/// Creates Supabase logins on the SACCO's behalf.
///
/// Members are vetted offline, so there is no public sign-up: an administrator enrols a member and
/// the account is created here, already confirmed, so the member can sign in straight away without
/// waiting for a confirmation email that may never arrive.
///
/// This needs the service-role key, which bypasses every access rule in the project. It is read
/// from server configuration only and must never be sent to a browser — if it appears in anything
/// prefixed NEXT_PUBLIC_, it has been published to the world.
/// </summary>
public class SupabaseAdminService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string? _url;
    private readonly string? _serviceRoleKey;

    public SupabaseAdminService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _url = configuration["SUPABASE_URL"]?.TrimEnd('/');
        _serviceRoleKey = configuration["SUPABASE_SERVICE_ROLE_KEY"];
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_url) && !string.IsNullOrWhiteSpace(_serviceRoleKey);

    public async Task<AdminResult> CreateUserAsync(
        string email, string? password, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return new AdminResult { Ok = false, Error = "SUPABASE_SERVICE_ROLE_KEY is not configured." };
        }

        // A member who is handed a password they did not choose should change it; a generated one
        // at least means no account is created with a guessable default.
        var initialPassword = string.IsNullOrWhiteSpace(password)
            ? GeneratePassword()
            : password;

        var payload = JsonSerializer.Serialize(new
        {
            email,
            password = initialPassword,
            email_confirm = true,
        });

        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_url}/auth/v1/admin/users")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            };
            request.Headers.TryAddWithoutValidation("apikey", _serviceRoleKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceRoleKey);

            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new AdminResult
                {
                    Ok = false,
                    Error = $"{(int)response.StatusCode} {response.ReasonPhrase}. {Summarize(body)}",
                };
            }

            return new AdminResult { Ok = true, GeneratedPassword = password is null ? initialPassword : null };
        }
        catch (Exception ex)
        {
            return new AdminResult { Ok = false, Error = $"{ex.GetType().Name}: {ex.Message}" };
        }
    }

    /// <summary>Keeps a provider error readable without dumping a whole response into a message.</summary>
    private static string Summarize(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return string.Empty;

        try
        {
            using var document = JsonDocument.Parse(body);
            foreach (var name in new[] { "msg", "message", "error_description", "error" })
            {
                if (document.RootElement.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString() ?? string.Empty;
                }
            }
        }
        catch (JsonException)
        {
            // Not JSON; fall through to the truncated body.
        }

        return body.Length > 200 ? body[..200] : body;
    }

    private static string GeneratePassword()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(20);
        return string.Concat(bytes.Select(b => alphabet[b % alphabet.Length]));
    }
}

public class AdminResult
{
    public bool Ok { get; init; }
    public string? Error { get; init; }

    /// <summary>Set only when no password was supplied and one had to be generated.</summary>
    public string? GeneratedPassword { get; init; }
}
