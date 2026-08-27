using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talanton.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanApplicationStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentStage",
                table: "LoanApplications",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentStage",
                table: "LoanApplications");
        }
    }
}
