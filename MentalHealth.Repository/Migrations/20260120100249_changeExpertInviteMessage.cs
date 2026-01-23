using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentalHealth.Repository.Migrations
{
    /// <inheritdoc />
    public partial class changeExpertInviteMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "ExpertInvitations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "ExpertInvitations");
        }
    }
}
