using Microsoft.EntityFrameworkCore;
using Talanton.Api.Models;

namespace Talanton.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // SACCO & Organizations
    public DbSet<Sacco> Saccos { get; set; }
    public DbSet<Cooperative> Cooperatives { get; set; }

    // Users & RBAC
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRoleAssignment> UserRoleAssignments { get; set; }

    // Applicants
    public DbSet<Applicant> Applicants { get; set; }
    public DbSet<ApplicantIndividualProfile> ApplicantIndividualProfiles { get; set; }
    public DbSet<ApplicantCooperativeProfile> ApplicantCooperativeProfiles { get; set; }
    public DbSet<SaccoMembership> SaccoMemberships { get; set; }

    // Loan Applications
    public DbSet<LoanApplication> LoanApplications { get; set; }
    public DbSet<LoanApplicationStatusHistory> LoanApplicationStatusHistories { get; set; }

    // Documents
    public DbSet<RequiredDocumentType> RequiredDocumentTypes { get; set; }
    public DbSet<ApplicationRequiredDocument> ApplicationRequiredDocuments { get; set; }
    public DbSet<ApplicationDocument> ApplicationDocuments { get; set; }
    public DbSet<DocumentVerification> DocumentVerifications { get; set; }

    // Credit & Underwriting
    public DbSet<CreditAssessment> CreditAssessments { get; set; }
    public DbSet<CreditKPI> CreditKPIs { get; set; }
    public DbSet<UnderwritingAssessment> UnderwritingAssessments { get; set; }

    // Committee
    public DbSet<CommitteeReview> CommitteeReviews { get; set; }
    public DbSet<CommitteeMembership> CommitteeMemberships { get; set; }
    public DbSet<CommitteeVote> CommitteeVotes { get; set; }

    // Decisions & Disbursement
    public DbSet<ApplicationDecision> ApplicationDecisions { get; set; }
    public DbSet<LoanDisbursement> LoanDisbursements { get; set; }

    // Audit
    public DbSet<AuditLog> AuditLogs { get; set; }

    // Ledger accounts
    public DbSet<LedgerAccount> LedgerAccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ================================
        // SACCO RELATIONSHIPS
        // ================================

        // SACCO -> Cooperative
        modelBuilder.Entity<Cooperative>()
            .HasOne(c => c.Sacco)
            .WithMany()
            .HasForeignKey(c => c.SaccoId)
            .OnDelete(DeleteBehavior.Restrict);

        // SACCO -> User
        modelBuilder.Entity<User>()
            .HasOne(u => u.Sacco)
            .WithMany()
            .HasForeignKey(u => u.SaccoId)
            .OnDelete(DeleteBehavior.Restrict);

        // SACCO -> Applicant
        modelBuilder.Entity<Applicant>()
            .HasOne(a => a.Sacco)
            .WithMany()
            .HasForeignKey(a => a.SaccoId)
            .OnDelete(DeleteBehavior.Restrict);

        // SACCO -> Loan Application
        modelBuilder.Entity<LoanApplication>()
            .HasOne(la => la.Sacco)
            .WithMany()
            .HasForeignKey(la => la.SaccoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Applicant profile one-to-one entities use ApplicantId as PK/FK.
        modelBuilder.Entity<ApplicantIndividualProfile>()
            .HasKey(p => p.ApplicantId);

        modelBuilder.Entity<ApplicantIndividualProfile>()
            .HasOne(p => p.Applicant)
            .WithOne(a => a.IndividualProfile)
            .HasForeignKey<ApplicantIndividualProfile>(p => p.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicantCooperativeProfile>()
            .HasKey(p => p.ApplicantId);

        modelBuilder.Entity<ApplicantCooperativeProfile>()
            .HasOne(p => p.Applicant)
            .WithOne(a => a.CooperativeProfile)
            .HasForeignKey<ApplicantCooperativeProfile>(p => p.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}