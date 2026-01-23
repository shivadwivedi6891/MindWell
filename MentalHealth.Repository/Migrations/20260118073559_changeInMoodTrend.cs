using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentalHealth.Repository.Migrations
{
    /// <inheritdoc />
    public partial class changeInMoodTrend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodStart",
                table: "MoodTrends",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PeriodStart",
                table: "MoodTrends");
        }
    }
}
