namespace Talanton.Api.Models;

public class CommitteeVote
{
    public Guid Id { get; set; }

    public Guid CommitteeReviewId { get; set; }

    public CommitteeReview CommitteeReview { get; set; } = null!;

    public Guid VoterUserId { get; set; }

    public User VoterUser { get; set; } = null!;

    public string Vote { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
}