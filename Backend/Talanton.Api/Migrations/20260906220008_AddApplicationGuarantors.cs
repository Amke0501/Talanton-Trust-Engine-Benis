using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talanton.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationGuarantors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationGuarantors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MemberId = table.Column<string>(type: "text", nullable: false),
                    PledgedShares = table.Column<decimal>(type: "numeric", nullable: false),
                    AvailableShares = table.Column<decimal>(type: "numeric", nullable: false),
                    LockedShares = table.Column<decimal>(type: "numeric", nullable: false),
                    SharesLockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SharesReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationGuarantors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationGuarantors_LoanApplications_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationGuarantors_LoanApplicationId_MemberId",
                table: "ApplicationGuarantors",
                columns: new[] { "LoanApplicationId", "MemberId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationGuarantors");
        }
    }
}
