using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talanton.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPersistedUnderwritingStateAndNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountRepaid",
                table: "LoanApplications",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeferredForLiquidityAt",
                table: "LoanApplications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeferredForLiquidityReason",
                table: "LoanApplications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisbursedAt",
                table: "LoanApplications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DtiNetRatio",
                table: "LoanApplications",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmergencyOverrideAt",
                table: "LoanApplications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyOverrideFirstSeat",
                table: "LoanApplications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyOverrideReason",
                table: "LoanApplications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyOverrideSecondSeat",
                table: "LoanApplications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GuardrailDepositMultiplierPassed",
                table: "LoanApplications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GuardrailGuarantorPassed",
                table: "LoanApplications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GuardrailOneThirdPayPassed",
                table: "LoanApplications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MemberId",
                table: "LoanApplications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyDebt",
                table: "LoanApplications",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyIncome",
                table: "LoanApplications",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Multiplier",
                table: "LoanApplications",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetTakeHome",
                table: "LoanApplications",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepaidAt",
                table: "LoanApplications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SavingsBalance",
                table: "LoanApplications",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "StatusNote",
                table: "LoanApplications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Verdict",
                table: "LoanApplications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Audience = table.Column<string>(type: "text", nullable: false),
                    AudienceKey = table.Column<string>(type: "text", nullable: true),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_MemberId",
                table: "LoanApplications",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Audience_CreatedAt",
                table: "Notifications",
                columns: new[] { "Audience", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_LoanApplications_MemberId",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "AmountRepaid",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "DeferredForLiquidityAt",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "DeferredForLiquidityReason",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "DisbursedAt",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "DtiNetRatio",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "EmergencyOverrideAt",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "EmergencyOverrideFirstSeat",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "EmergencyOverrideReason",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "EmergencyOverrideSecondSeat",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "GuardrailDepositMultiplierPassed",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "GuardrailGuarantorPassed",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "GuardrailOneThirdPayPassed",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "MonthlyDebt",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "MonthlyIncome",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "Multiplier",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "NetTakeHome",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "RepaidAt",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "SavingsBalance",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "StatusNote",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "Verdict",
                table: "LoanApplications");
        }
    }
}
