using Talanton.Api.Services;

namespace Talanton.Api.Tests;

/// <summary>
/// Passwords were stored and compared in plain text. These cover the replacement, including the
/// path that lets existing accounts keep working while their stored value is upgraded.
/// </summary>
public class PasswordHasherTests
{
    [Fact]
    public void A_hash_verifies_against_its_own_password()
    {
        var stored = PasswordHasher.Hash("Demo123!");
        Assert.True(PasswordHasher.Verify("Demo123!", stored, out var needsRehash));
        Assert.False(needsRehash);
    }

    [Fact]
    public void A_wrong_password_is_rejected()
    {
        var stored = PasswordHasher.Hash("Demo123!");
        Assert.False(PasswordHasher.Verify("Demo123", stored, out _));
    }

    [Fact]
    public void The_password_itself_never_appears_in_the_stored_value()
    {
        var stored = PasswordHasher.Hash("Demo123!");
        Assert.DoesNotContain("Demo123!", stored);
    }

    [Fact]
    public void The_same_password_hashes_differently_every_time()
    {
        // A per-password salt: two members choosing the same password must not be visibly equal
        // in the table, and the hashes must not be precomputable.
        Assert.NotEqual(PasswordHasher.Hash("Demo123!"), PasswordHasher.Hash("Demo123!"));
    }

    [Fact]
    public void A_legacy_plaintext_password_still_verifies_and_is_flagged_for_upgrade()
    {
        Assert.True(PasswordHasher.Verify("Demo123!", "Demo123!", out var needsRehash));
        Assert.True(needsRehash);
    }

    [Fact]
    public void A_wrong_password_against_a_legacy_store_is_rejected_and_not_upgraded()
    {
        Assert.False(PasswordHasher.Verify("wrong", "Demo123!", out var needsRehash));
        Assert.False(needsRehash);
    }

    [Fact]
    public void Legacy_plaintext_is_recognised_and_a_real_hash_is_not()
    {
        Assert.True(PasswordHasher.IsLegacyPlaintext("Demo123!"));
        Assert.False(PasswordHasher.IsLegacyPlaintext(PasswordHasher.Hash("Demo123!")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Missing_input_never_verifies(string? password)
    {
        Assert.False(PasswordHasher.Verify(password, PasswordHasher.Hash("Demo123!"), out _));
        Assert.False(PasswordHasher.Verify("Demo123!", password, out _));
    }

    [Theory]
    [InlineData("pbkdf2.sha256$notanumber$c2FsdA==$aGFzaA==")]
    [InlineData("pbkdf2.sha256$210000$!!!notbase64!!!$aGFzaA==")]
    [InlineData("pbkdf2.sha256$210000$c2FsdA==")]
    public void A_malformed_stored_value_is_rejected_rather_than_throwing(string stored)
    {
        Assert.False(PasswordHasher.Verify("Demo123!", stored, out _));
    }
}
