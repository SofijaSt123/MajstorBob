using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Majstor_bob.Migrations
{
    /// <inheritdoc />
    public partial class TipOpciono : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "tip_kome_saljes",
                table: "obavestenja",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "tip_kome_saljes",
                table: "obavestenja",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
