using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentalHealth.Repository.Migrations
{
    /// <inheritdoc />
    public partial class changeExpertInvite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ExpertInvitations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ExpertInvitations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ExpertInvitations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ExpertInvitations");
        }
    }
}
