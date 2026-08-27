using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talanton.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCounterOfferConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicantConsentAt",
                table: "LoanApplications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApplicantConsentReceived",
                table: "LoanApplications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CounterOfferPrincipalAmount",
                table: "LoanApplications",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CounterOfferReason",
                table: "LoanApplications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CounterOfferStatus",
                table: "LoanApplications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CounterOfferTermMonths",
                table: "LoanApplications",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicantConsentAt",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "ApplicantConsentReceived",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "CounterOfferPrincipalAmount",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "CounterOfferReason",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "CounterOfferStatus",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "CounterOfferTermMonths",
                table: "LoanApplications");
        }
    }
}
