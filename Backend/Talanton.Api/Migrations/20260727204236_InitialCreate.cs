using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talanton.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Saccos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ContactEmail = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Saccos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cooperatives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "text", nullable: false),
                    ContactPersonName = table.Column<string>(type: "text", nullable: true),
                    ContactPhone = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    SaccoId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cooperatives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cooperatives_Saccos_SaccoId",
                        column: x => x.SaccoId,
                        principalTable: "Saccos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequiredDocumentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ApplicantType = table.Column<string>(type: "text", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    AllowedMimeTypes = table.Column<string>(type: "text", nullable: true),
                    MaxFileSizeMb = table.Column<int>(type: "integer", nullable: false),
                    IsCollateralDocumentType = table.Column<bool>(type: "boolean", nullable: false),
                    SaccoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequiredDocumentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequiredDocumentTypes_Saccos_SaccoId",
                        column: x => x.SaccoId,
                        principalTable: "Saccos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SaccoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Saccos_SaccoId",
                        column: x => x.SaccoId,
                        principalTable: "Saccos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Applicants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantType = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SaccoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applicants_Saccos_SaccoId",
                        column: x => x.SaccoId,
                        principalTable: "Saccos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Applicants_Users_ApplicantUserId",
                        column: x => x.ApplicantUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    BeforeData = table.Column<string>(type: "text", nullable: true),
                    AfterData = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SaccoId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoleAssignments_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoleAssignments_Saccos_SaccoId",
                        column: x => x.SaccoId,
                        principalTable: "Saccos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserRoleAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantCooperativeProfiles",
                columns: table => new
                {
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CooperativeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorizedSignatoryName = table.Column<string>(type: "text", nullable: true),
                    AuthorizedSignatoryPhone = table.Column<string>(type: "text", nullable: true),
                    MembershipCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantCooperativeProfiles", x => x.ApplicantId);
                    table.ForeignKey(
                        name: "FK_ApplicantCooperativeProfiles_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicantCooperativeProfiles_Cooperatives_CooperativeId",
                        column: x => x.CooperativeId,
                        principalTable: "Cooperatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantIndividualProfiles",
                columns: table => new
                {
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NationalIdNumber = table.Column<string>(type: "text", nullable: true),
                    EmploymentType = table.Column<string>(type: "text", nullable: true),
                    MonthlyIncome = table.Column<decimal>(type: "numeric", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantIndividualProfiles", x => x.ApplicantId);
                    table.ForeignKey(
                        name: "FK_ApplicantIndividualProfiles_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoanApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationNumber = table.Column<string>(type: "text", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SaccoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedUnderwriterUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedUnderwriterId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStatus = table.Column<string>(type: "text", nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    AnnualSimpleInterestRatePct = table.Column<decimal>(type: "numeric", nullable: false),
                    TermMonths = table.Column<int>(type: "integer", nullable: false),
                    AdministrativeFeeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Purpose = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanApplications_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoanApplications_Saccos_SaccoId",
                        column: x => x.SaccoId,
                        principalTable: "Saccos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanApplications_Users_AssignedUnderwriterId",
                        column: x => x.AssignedUnderwriterId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LoanApplications_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaccoMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SaccoId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipNumber = table.Column<string>(type: "text", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    IsPrimaryMember = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaccoMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaccoMemberships_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaccoMemberships_Saccos_SaccoId",
                        column: x => x.SaccoId,
                        principalTable: "Saccos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentName = table.Column<string>(type: "text", nullable: false),
                    StorageUri = table.Column<string>(type: "text", nullable: false),
                    FileHash = table.Column<string>(type: "text", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationDocuments_LoanApplications_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationDocuments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationRequiredDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequiredDocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsFulfilled = table.Column<bool>(type: "boolean", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRequiredDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationRequiredDocuments_LoanApplications_LoanApplicati~",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationRequiredDocuments_RequiredDocumentTypes_Required~",
                        column: x => x.RequiredDocumentTypeId,
                        principalTable: "RequiredDocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewStatus = table.Column<string>(type: "text", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QuorumRequired = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitteeReviews_LoanApplications_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommitteeReviews_Users_InitiatedByUserId",
                        column: x => x.InitiatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssessmentVersion = table.Column<int>(type: "integer", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: true),
                    OverallCreditScore = table.Column<decimal>(type: "numeric", nullable: true),
                    RiskGrade = table.Column<string>(type: "text", nullable: true),
                    NarrativeSummary = table.Column<string>(type: "text", nullable: true),
                    IsFinal = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditAssessments_LoanApplications_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditAssessments_Users_AssessedByUserId",
                        column: x => x.AssessedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LoanApplicationStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "text", nullable: true),
                    ToStatus = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanApplicationStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanApplicationStatusHistories_LoanApplications_LoanApplica~",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoanApplicationStatusHistories_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerificationStatus = table.Column<string>(type: "text", nullable: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentVerifications_ApplicationDocuments_ApplicationDocum~",
                        column: x => x.ApplicationDocumentId,
                        principalTable: "ApplicationDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentVerifications_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApplicationDecisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommitteeReviewId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinalDecision = table.Column<string>(type: "text", nullable: false),
                    DecisionReason = table.Column<string>(type: "text", nullable: true),
                    ApprovedPrincipal = table.Column<decimal>(type: "numeric", nullable: true),
                    ApprovedAnnualSimpleInterestRatePct = table.Column<decimal>(type: "numeric", nullable: true),
                    ApprovedTermMonths = table.Column<int>(type: "integer", nullable: true),
                    ApprovedAdministrativeFeeAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    DecisionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationDecisions_CommitteeReviews_CommitteeReviewId",
                        column: x => x.CommitteeReviewId,
                        principalTable: "CommitteeReviews",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationDecisions_LoanApplications_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationDecisions_Users_DecidedByUserId",
                        column: x => x.DecidedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SaccoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommitteeReviewId = table.Column<Guid>(type: "uuid", nullable: true),
                    MembershipStatus = table.Column<string>(type: "text", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CanVote = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitteeMemberships_CommitteeReviews_CommitteeReviewId",
                        column: x => x.CommitteeReviewId,
                        principalTable: "CommitteeReviews",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommitteeMemberships_Saccos_SaccoId",
                        column: x => x.SaccoId,
                        principalTable: "Saccos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommitteeMemberships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommitteeReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Vote = table.Column<string>(type: "text", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    VotedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitteeVotes_CommitteeReviews_CommitteeReviewId",
                        column: x => x.CommitteeReviewId,
                        principalTable: "CommitteeReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommitteeVotes_Users_VoterUserId",
                        column: x => x.VoterUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditKPIs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditAssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    KpiCode = table.Column<string>(type: "text", nullable: false),
                    KpiName = table.Column<string>(type: "text", nullable: false),
                    KpiValue = table.Column<decimal>(type: "numeric", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: true),
                    CalculationMethodVersion = table.Column<string>(type: "text", nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditKPIs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditKPIs_CreditAssessments_CreditAssessmentId",
                        column: x => x.CreditAssessmentId,
                        principalTable: "CreditAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnderwritingAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnderwriterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditAssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Recommendation = table.Column<string>(type: "text", nullable: false),
                    RecommendedPrincipal = table.Column<decimal>(type: "numeric", nullable: true),
                    RecommendedAnnualSimpleInterestRatePct = table.Column<decimal>(type: "numeric", nullable: true),
                    RecommendedTermMonths = table.Column<int>(type: "integer", nullable: true),
                    RecommendedAdministrativeFeeAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    Conditions = table.Column<string>(type: "text", nullable: true),
                    Rationale = table.Column<string>(type: "text", nullable: true),
                    AssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnderwritingAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnderwritingAssessments_CreditAssessments_CreditAssessmentId",
                        column: x => x.CreditAssessmentId,
                        principalTable: "CreditAssessments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UnderwritingAssessments_LoanApplications_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnderwritingAssessments_Users_UnderwriterUserId",
                        column: x => x.UnderwriterUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoanDisbursements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationDecisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisbursementStatus = table.Column<string>(type: "text", nullable: false),
                    PrincipalDisbursedAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    AdministrativeFeeDeductedAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    NetAmountToBorrower = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentChannel = table.Column<string>(type: "text", nullable: true),
                    TransactionReference = table.Column<string>(type: "text", nullable: true),
                    DisbursedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanDisbursements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanDisbursements_ApplicationDecisions_ApplicationDecisionId",
                        column: x => x.ApplicationDecisionId,
                        principalTable: "ApplicationDecisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoanDisbursements_LoanApplications_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoanDisbursements_Users_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantCooperativeProfiles_CooperativeId",
                table: "ApplicantCooperativeProfiles",
                column: "CooperativeId");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_ApplicantUserId",
                table: "Applicants",
                column: "ApplicantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_SaccoId",
                table: "Applicants",
                column: "SaccoId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDecisions_CommitteeReviewId",
                table: "ApplicationDecisions",
                column: "CommitteeReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDecisions_DecidedByUserId",
                table: "ApplicationDecisions",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDecisions_LoanApplicationId",
                table: "ApplicationDecisions",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocuments_LoanApplicationId",
                table: "ApplicationDocuments",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocuments_UploadedByUserId",
                table: "ApplicationDocuments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRequiredDocuments_LoanApplicationId",
                table: "ApplicationRequiredDocuments",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRequiredDocuments_RequiredDocumentTypeId",
                table: "ApplicationRequiredDocuments",
                column: "RequiredDocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorUserId",
                table: "AuditLogs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeMemberships_CommitteeReviewId",
                table: "CommitteeMemberships",
                column: "CommitteeReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeMemberships_SaccoId",
                table: "CommitteeMemberships",
                column: "SaccoId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeMemberships_UserId",
                table: "CommitteeMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeReviews_InitiatedByUserId",
                table: "CommitteeReviews",
                column: "InitiatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeReviews_LoanApplicationId",
                table: "CommitteeReviews",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeVotes_CommitteeReviewId",
                table: "CommitteeVotes",
                column: "CommitteeReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeVotes_VoterUserId",
                table: "CommitteeVotes",
                column: "VoterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Cooperatives_SaccoId",
                table: "Cooperatives",
                column: "SaccoId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditAssessments_AssessedByUserId",
                table: "CreditAssessments",
                column: "AssessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditAssessments_LoanApplicationId",
                table: "CreditAssessments",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditKPIs_CreditAssessmentId",
                table: "CreditKPIs",
                column: "CreditAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVerifications_ApplicationDocumentId",
                table: "DocumentVerifications",
                column: "ApplicationDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVerifications_VerifiedByUserId",
                table: "DocumentVerifications",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_ApplicantId",
                table: "LoanApplications",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_AssignedUnderwriterId",
                table: "LoanApplications",
                column: "AssignedUnderwriterId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_CreatedByUserId",
                table: "LoanApplications",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_SaccoId",
                table: "LoanApplications",
                column: "SaccoId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplicationStatusHistories_ChangedByUserId",
                table: "LoanApplicationStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplicationStatusHistories_LoanApplicationId",
                table: "LoanApplicationStatusHistories",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanDisbursements_ApplicationDecisionId",
                table: "LoanDisbursements",
                column: "ApplicationDecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanDisbursements_LoanApplicationId",
                table: "LoanDisbursements",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanDisbursements_ProcessedByUserId",
                table: "LoanDisbursements",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequiredDocumentTypes_SaccoId",
                table: "RequiredDocumentTypes",
                column: "SaccoId");

            migrationBuilder.CreateIndex(
                name: "IX_SaccoMemberships_ApplicantId",
                table: "SaccoMemberships",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_SaccoMemberships_SaccoId",
                table: "SaccoMemberships",
                column: "SaccoId");

            migrationBuilder.CreateIndex(
                name: "IX_UnderwritingAssessments_CreditAssessmentId",
                table: "UnderwritingAssessments",
                column: "CreditAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UnderwritingAssessments_LoanApplicationId",
                table: "UnderwritingAssessments",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_UnderwritingAssessments_UnderwriterUserId",
                table: "UnderwritingAssessments",
                column: "UnderwriterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_RoleId",
                table: "UserRoleAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_SaccoId",
                table: "UserRoleAssignments",
                column: "SaccoId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_UserId",
                table: "UserRoleAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SaccoId",
                table: "Users",
                column: "SaccoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicantCooperativeProfiles");

            migrationBuilder.DropTable(
                name: "ApplicantIndividualProfiles");

            migrationBuilder.DropTable(
                name: "ApplicationRequiredDocuments");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CommitteeMemberships");

            migrationBuilder.DropTable(
                name: "CommitteeVotes");

            migrationBuilder.DropTable(
                name: "CreditKPIs");

            migrationBuilder.DropTable(
                name: "DocumentVerifications");

            migrationBuilder.DropTable(
                name: "LoanApplicationStatusHistories");

            migrationBuilder.DropTable(
                name: "LoanDisbursements");

            migrationBuilder.DropTable(
                name: "SaccoMemberships");

            migrationBuilder.DropTable(
                name: "UnderwritingAssessments");

            migrationBuilder.DropTable(
                name: "UserRoleAssignments");

            migrationBuilder.DropTable(
                name: "Cooperatives");

            migrationBuilder.DropTable(
                name: "RequiredDocumentTypes");

            migrationBuilder.DropTable(
                name: "ApplicationDocuments");

            migrationBuilder.DropTable(
                name: "ApplicationDecisions");

            migrationBuilder.DropTable(
                name: "CreditAssessments");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "CommitteeReviews");

            migrationBuilder.DropTable(
                name: "LoanApplications");

            migrationBuilder.DropTable(
                name: "Applicants");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Saccos");
        }
    }
}
