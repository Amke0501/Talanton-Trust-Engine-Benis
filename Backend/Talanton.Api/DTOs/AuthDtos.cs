namespace Talanton.Api.DTOs;

/// <summary>
/// Who the caller is, resolved from a verified Supabase token against the SACCO's own records.
/// The portal and the committee seat are answers, not requests: they used to be chosen by the
/// person signing in.
/// </summary>
public class CurrentUserDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    /// <summary>applicant | underwriter | committee</summary>
    public string PortalRole { get; set; } = string.Empty;

    /// <summary>The board seat, for committee members only.</summary>
    public string? CommitteeSeat { get; set; }

    /// <summary>The applicant's membership number, where they have one.</summary>
    public string? MemberId { get; set; }
}

/// <summary>An administrator enrolling a member and creating the login that goes with it.</summary>
public class CreateAccountDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    /// <summary>applicant | underwriter | committee</summary>
    public string PortalRole { get; set; } = string.Empty;

    /// <summary>Required for a committee member; decides whose vote counts and who may release funds.</summary>
    public string? CommitteeSeat { get; set; }

    /// <summary>Optional. One is generated when omitted, and the member should change it.</summary>
    public string? Password { get; set; }
}

public class DevTokenRequestDto
{
    public string Email { get; set; } = string.Empty;
}
