using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Talanton.Api.Services;

/// <summary>
/// Creates Supabase logins on the SACCO's behalf.
///
/// Members are vetted offline, so there is no public sign-up: an administrator enrols a member and
/// the account is created here, already usable, rather than waiting on a confirmation email that
/// may never arrive.
///
/// There are two ways to do that and this prefers the better one. With a service-role key it uses
/// the admin API, which creates the account confirmed regardless of project settings. Without one
/// it falls back to ordinary sign-up, which only produces a usable account while the project has
/// email auto-confirm switched on — so the fallback reports exactly what it did, and the caller
/// passes that on rather than claiming a success it cannot verify.
///
/// The service-role key bypasses every access rule in the project. It is read from server
/// configuration only and must never reach a browser: anything prefixed NEXT_PUBLIC_ is published.
/// </summary>
public class SupabaseAdminService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string? _url;
    private readonly string? _serviceRoleKey;
    private readonly string? _anonKey;

    public SupabaseAdminService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _url = FirstSet(configuration["SUPABASE_URL"], configuration["NEXT_PUBLIC_SUPABASE_URL"])?.TrimEnd('/');
        _serviceRoleKey = configuration["SUPABASE_SERVICE_ROLE_KEY"];
        _anonKey = FirstSet(configuration["SUPABASE_ANON_KEY"], configuration["NEXT_PUBLIC_SUPABASE_ANON_KEY"]);
    }

    /// <summary>Whether an account can be created at all, by either route.</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_url)
        && (!string.IsNullOrWhiteSpace(_serviceRoleKey) || !string.IsNullOrWhiteSpace(_anonKey));

    /// <summary>True when the stronger route is available.</summary>
    public bool HasAdminKey => !string.IsNullOrWhiteSpace(_serviceRoleKey);

    public string NotConfiguredMessage =>
        "Account creation is not configured. Set SUPABASE_SERVICE_ROLE_KEY on the API (preferred), " +
        "or SUPABASE_ANON_KEY with email auto-confirm enabled on the project. Failing that, create " +
        "the login in the Supabase dashboard using the same email address.";

    public async Task<AdminResult> CreateUserAsync(
        string email, string? password, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return new AdminResult { Ok = false, Error = NotConfiguredMessage };
        }

        // A password nobody chose beats a password everybody can guess.
        var generated = string.IsNullOrWhiteSpace(password);
        var initialPassword = generated ? GeneratePassword() : password!;

        return HasAdminKey
            ? await CreateViaAdminApiAsync(email, initialPassword, generated, cancellationToken)
            : await CreateViaSignUpAsync(email, initialPassword, generated, cancellationToken);
    }

    /// <summary>The preferred route: creates the account already confirmed, sends no email.</summary>
    private async Task<AdminResult> CreateViaAdminApiAsync(
        string email, string password, bool generated, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new { email, password, email_confirm = true });

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
                return new AdminResult { Ok = false, Error = Summarize(body) };
            }

            return new AdminResult
            {
                Ok = true,
                InitialPassword = generated ? password : null,
                CanSignInImmediately = true,
            };
        }
        catch (Exception ex)
        {
            return new AdminResult { Ok = false, Error = $"{ex.GetType().Name}: {ex.Message}" };
        }
    }

    /// <summary>
    /// The fallback: an ordinary sign-up with the project's public key. Whether the resulting
    /// account can actually be used depends on the project's confirmation setting, so rather than
    /// guess, this tries to sign in as the new account and reports the answer.
    /// </summary>
    private async Task<AdminResult> CreateViaSignUpAsync(
        string email, string password, bool generated, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            using var signUp = new HttpRequestMessage(HttpMethod.Post, $"{_url}/auth/v1/signup")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { email, password }), Encoding.UTF8, "application/json"),
            };
            signUp.Headers.TryAddWithoutValidation("apikey", _anonKey);

            using var response = await client.SendAsync(signUp, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new AdminResult { Ok = false, Error = Summarize(body) };
            }

            var usable = await CanSignInAsync(client, email, password, cancellationToken);

            return new AdminResult
            {
                Ok = true,
                InitialPassword = generated ? password : null,
                CanSignInImmediately = usable,
                Error = usable
                    ? null
                    : "The login was created but cannot be used yet: the project requires email " +
                      "confirmation. Confirm it in the Supabase dashboard, or set " +
                      "SUPABASE_SERVICE_ROLE_KEY so accounts are created already confirmed.",
            };
        }
        catch (Exception ex)
        {
            return new AdminResult { Ok = false, Error = $"{ex.GetType().Name}: {ex.Message}" };
        }
    }

    /// <summary>The only proof that an account works: use it.</summary>
    private async Task<bool> CanSignInAsync(
        HttpClient client, string email, string password, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post, $"{_url}/auth/v1/token?grant_type=password")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { email, password }), Encoding.UTF8, "application/json"),
            };
            request.Headers.TryAddWithoutValidation("apikey", _anonKey);

            using var response = await client.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Keeps a provider error readable without dumping a whole response into a message.</summary>
    private static string Summarize(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return "The identity provider gave no reason.";

        try
        {
            using var document = JsonDocument.Parse(body);
            foreach (var name in new[] { "msg", "message", "error_description", "error" })
            {
                if (document.RootElement.TryGetProperty(name, out var value)
                    && value.ValueKind == JsonValueKind.String)
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
        // Deliberately excludes characters that are easy to misread aloud or in handwriting,
        // because this password gets read out or written down before the member changes it.
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(14);
        return string.Concat(bytes.Select(b => alphabet[b % alphabet.Length])) + "!7";
    }

    private static string? FirstSet(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}

public class AdminResult
{
    public bool Ok { get; init; }
    public string? Error { get; init; }

    /// <summary>Set only when no password was supplied and one had to be generated.</summary>
    public string? InitialPassword { get; init; }

    /// <summary>False when the account exists but still needs confirming before it can be used.</summary>
    public bool CanSignInImmediately { get; init; }
}
