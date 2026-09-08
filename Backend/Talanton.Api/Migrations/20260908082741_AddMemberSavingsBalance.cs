using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talanton.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberSavingsBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SavingsBalance",
                table: "SaccoMemberships",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "SavingsUpdatedAt",
                table: "SaccoMemberships",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SavingsBalance",
                table: "SaccoMemberships");

            migrationBuilder.DropColumn(
                name: "SavingsUpdatedAt",
                table: "SaccoMemberships");
        }
    }
}
