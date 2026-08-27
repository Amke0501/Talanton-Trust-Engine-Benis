namespace Talanton.Api.Models;

public class CommitteeMembership
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid SaccoId { get; set; }

    public Sacco Sacco { get; set; } = null!;

    public Guid? CommitteeReviewId { get; set; }

    public CommitteeReview? CommitteeReview { get; set; }

    public string MembershipStatus { get; set; } = "Active";

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool CanVote { get; set; } = true;
}