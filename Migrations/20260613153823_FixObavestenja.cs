using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Majstor_bob.Migrations
{
    /// <inheritdoc />
    public partial class FixObavestenja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_obavestenja_korisnici_korisnik_id",
                table: "obavestenja");

            migrationBuilder.DropForeignKey(
                name: "FK_obavestenja_korisnici_povezani_id",
                table: "obavestenja");

            migrationBuilder.DropIndex(
                name: "IX_obavestenja_povezani_id",
                table: "obavestenja");

            migrationBuilder.DropColumn(
                name: "procitano",
                table: "obavestenja");

            migrationBuilder.RenameColumn(
                name: "povezani_id",
                table: "obavestenja",
                newName: "target_group");

            migrationBuilder.RenameColumn(
                name: "korisnik_id",
                table: "obavestenja",
                newName: "admin_id");

            migrationBuilder.RenameIndex(
                name: "IX_obavestenja_korisnik_id",
                table: "obavestenja",
                newName: "IX_obavestenja_admin_id");

            migrationBuilder.AddColumn<int>(
                name: "receiver_id",
                table: "obavestenja",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_obavestenja_receiver_id",
                table: "obavestenja",
                column: "receiver_id");

            migrationBuilder.AddForeignKey(
                name: "FK_obavestenja_korisnici_admin_id",
                table: "obavestenja",
                column: "admin_id",
                principalTable: "korisnici",
                principalColumn: "id_korisnik",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_obavestenja_korisnici_receiver_id",
                table: "obavestenja",
                column: "receiver_id",
                principalTable: "korisnici",
                principalColumn: "id_korisnik",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_obavestenja_korisnici_admin_id",
                table: "obavestenja");

            migrationBuilder.DropForeignKey(
                name: "FK_obavestenja_korisnici_receiver_id",
                table: "obavestenja");

            migrationBuilder.DropIndex(
                name: "IX_obavestenja_receiver_id",
                table: "obavestenja");

            migrationBuilder.DropColumn(
                name: "receiver_id",
                table: "obavestenja");

            migrationBuilder.RenameColumn(
                name: "target_group",
                table: "obavestenja",
                newName: "povezani_id");

            migrationBuilder.RenameColumn(
                name: "admin_id",
                table: "obavestenja",
                newName: "korisnik_id");

            migrationBuilder.RenameIndex(
                name: "IX_obavestenja_admin_id",
                table: "obavestenja",
                newName: "IX_obavestenja_korisnik_id");

            migrationBuilder.AddColumn<bool>(
                name: "procitano",
                table: "obavestenja",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_obavestenja_povezani_id",
                table: "obavestenja",
                column: "povezani_id");

            migrationBuilder.AddForeignKey(
                name: "FK_obavestenja_korisnici_korisnik_id",
                table: "obavestenja",
                column: "korisnik_id",
                principalTable: "korisnici",
                principalColumn: "id_korisnik",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_obavestenja_korisnici_povezani_id",
                table: "obavestenja",
                column: "povezani_id",
                principalTable: "korisnici",
                principalColumn: "id_korisnik",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
