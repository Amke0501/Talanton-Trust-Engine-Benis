using System.Security.Cryptography;

namespace Talanton.Api.Services;

/// <summary>
/// Hashes and verifies account passwords.
///
/// Passwords were previously stored and compared in plain text, so anyone with read access to the
/// Users table held every member's credentials. This uses PBKDF2-HMAC-SHA256 with a per-password
/// random salt — available in the framework, so no new dependency — and a constant-time comparison
/// so a wrong guess cannot be narrowed down by how long the check took.
///
/// Stored format: pbkdf2.sha256${iterations}${salt}${hash}, all base64. Keeping the parameters in
/// the string means the work factor can be raised later without invalidating existing passwords.
/// </summary>
public static class PasswordHasher
{
    private const string Prefix = "pbkdf2.sha256";
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    /// <summary>OWASP's floor for PBKDF2-HMAC-SHA256 at time of writing.</summary>
    private const int Iterations = 210_000;

    public static string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashBytes);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Checks a password against a stored value.
    /// </summary>
    /// <param name="needsRehash">
    /// True when the stored value is a legacy plaintext password, or was hashed with a lower work
    /// factor than we now use. The caller should re-store the password on a successful login.
    /// </param>
    public static bool Verify(string? password, string? stored, out bool needsRehash)
    {
        needsRehash = false;
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(stored)) return false;

        var parts = stored.Split('$');

        // Anything that is not in our format is a password from before hashing existed. Compare it
        // as plaintext so existing accounts keep working, and flag it for immediate upgrade.
        if (parts.Length != 4 || parts[0] != Prefix)
        {
            var matches = CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(password),
                System.Text.Encoding.UTF8.GetBytes(stored));
            needsRehash = matches;
            return matches;
        }

        if (!int.TryParse(parts[1], out var iterations) || iterations <= 0) return false;

        byte[] salt, expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        var ok = CryptographicOperations.FixedTimeEquals(actual, expected);

        if (ok && iterations < Iterations) needsRehash = true;
        return ok;
    }

    /// <summary>Whether a stored value is still an unhashed password.</summary>
    public static bool IsLegacyPlaintext(string? stored)
        => !string.IsNullOrEmpty(stored) && !stored.StartsWith(Prefix + "$", StringComparison.Ordinal);
}
