using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Majstor_bob.Migrations
{
    /// <inheritdoc />
    public partial class FixObavestenja2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "procitanno",
                table: "obavestenja",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "procitanno",
                table: "obavestenja");
        }
    }
}
