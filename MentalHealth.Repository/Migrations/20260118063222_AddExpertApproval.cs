using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentalHealth.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddExpertApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "ExpertProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByAdminId",
                table: "ExpertProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "ExpertProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "ExpertProfiles");

            migrationBuilder.DropColumn(
                name: "ApprovedByAdminId",
                table: "ExpertProfiles");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "ExpertProfiles");
        }
    }
}
