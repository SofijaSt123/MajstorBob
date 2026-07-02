using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Majstor_bob.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gradovi",
                columns: table => new
                {
                    id_grad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    naziv_grada = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    postanski_broj = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gradovi", x => x.id_grad);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "kategorije",
                columns: table => new
                {
                    id_kategorije = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_roditelja = table.Column<int>(type: "int", nullable: true),
                    naziv_kategorije = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kategorije", x => x.id_kategorije);
                    table.ForeignKey(
                        name: "FK_kategorije_kategorije_id_roditelja",
                        column: x => x.id_roditelja,
                        principalTable: "kategorije",
                        principalColumn: "id_kategorije",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "korisnici",
                columns: table => new
                {
                    id_korisnik = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    email = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lozinka = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ime = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    prezime = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    profilna_slika = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tip_korisnika = table.Column<int>(type: "int", nullable: false),
                    verifikovan = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    aktivan = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    datum_registacije = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    poslednji_login = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    refresh_token = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    token_istice = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    blokiran = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_flaged = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_korisnici", x => x.id_korisnik);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "firme",
                columns: table => new
                {
                    id_firme = table.Column<int>(type: "int", nullable: false),
                    naziv_firme = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    broj_dostupnih_radnika = table.Column<int>(type: "int", nullable: false),
                    broj_ukupnih_radnika = table.Column<int>(type: "int", nullable: false),
                    opis_usluga = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pocetak_radnog_vremena = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    kraj_radnog_vremena = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    pib = table.Column<int>(type: "int", nullable: true),
                    prosek_ocena = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    broj_recenzija = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_firme", x => x.id_firme);
                    table.ForeignKey(
                        name: "FK_firme_korisnici_id_firme",
                        column: x => x.id_firme,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gradovi_rada",
                columns: table => new
                {
                    id_grad_rada = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_izvodjaca = table.Column<int>(type: "int", nullable: false),
                    id_grada = table.Column<int>(type: "int", nullable: false),
                    zona_rada = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    doplata = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gradovi_rada", x => x.id_grad_rada);
                    table.ForeignKey(
                        name: "FK_gradovi_rada_gradovi_id_grada",
                        column: x => x.id_grada,
                        principalTable: "gradovi",
                        principalColumn: "id_grad",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gradovi_rada_korisnici_id_izvodjaca",
                        column: x => x.id_izvodjaca,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "klijenti",
                columns: table => new
                {
                    id_klijent = table.Column<int>(type: "int", nullable: false),
                    id_grad_rada = table.Column<int>(type: "int", nullable: true),
                    gradGdeKlijentRadid_grad = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_klijenti", x => x.id_klijent);
                    table.ForeignKey(
                        name: "FK_klijenti_gradovi_gradGdeKlijentRadid_grad",
                        column: x => x.gradGdeKlijentRadid_grad,
                        principalTable: "gradovi",
                        principalColumn: "id_grad");
                    table.ForeignKey(
                        name: "FK_klijenti_gradovi_id_grad_rada",
                        column: x => x.id_grad_rada,
                        principalTable: "gradovi",
                        principalColumn: "id_grad");
                    table.ForeignKey(
                        name: "FK_klijenti_korisnici_id_klijent",
                        column: x => x.id_klijent,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "majstori",
                columns: table => new
                {
                    id_majstora = table.Column<int>(type: "int", nullable: false),
                    opis_usluga = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pocetak_radnog_vremena = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    kraj_radnog_vremena = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    jmbg = table.Column<int>(type: "int", nullable: true),
                    prosek_ocena = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    broj_recenzija = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_majstori", x => x.id_majstora);
                    table.ForeignKey(
                        name: "FK_majstori_korisnici_id_majstora",
                        column: x => x.id_majstora,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "obavestenja",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    korisnik_id = table.Column<int>(type: "int", nullable: false),
                    tip = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    naslov = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    povezani_id = table.Column<int>(type: "int", nullable: false),
                    procitano = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obavestenja", x => x.id);
                    table.ForeignKey(
                        name: "FK_obavestenja_korisnici_korisnik_id",
                        column: x => x.korisnik_id,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_obavestenja_korisnici_povezani_id",
                        column: x => x.povezani_id,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pripada",
                columns: table => new
                {
                    id_pripada = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_izvodjaca = table.Column<int>(type: "int", nullable: false),
                    id_kategorije = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pripada", x => x.id_pripada);
                    table.ForeignKey(
                        name: "FK_pripada_kategorije_id_kategorije",
                        column: x => x.id_kategorije,
                        principalTable: "kategorije",
                        principalColumn: "id_kategorije",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pripada_korisnici_id_izvodjaca",
                        column: x => x.id_izvodjaca,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "zapisnik_admin",
                columns: table => new
                {
                    id_zapisnik = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_admina = table.Column<int>(type: "int", nullable: false),
                    ip_adresa = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    akcija = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    datum = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zapisnik_admin", x => x.id_zapisnik);
                    table.ForeignKey(
                        name: "FK_zapisnik_admin_korisnici_id_admina",
                        column: x => x.id_admina,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ocene",
                columns: table => new
                {
                    id_ocene = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_klijenta = table.Column<int>(type: "int", nullable: false),
                    id_izvodjaca = table.Column<int>(type: "int", nullable: false),
                    opis_recenzije = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    odgovor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ocena_cena = table.Column<int>(type: "int", nullable: false),
                    ocena_kvaliteta = table.Column<int>(type: "int", nullable: false),
                    ocena_brzine = table.Column<int>(type: "int", nullable: false),
                    ocena_odnosa = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ocene", x => x.id_ocene);
                    table.ForeignKey(
                        name: "FK_ocene_klijenti_id_klijenta",
                        column: x => x.id_klijenta,
                        principalTable: "klijenti",
                        principalColumn: "id_klijent",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ocene_korisnici_id_izvodjaca",
                        column: x => x.id_izvodjaca,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "prijave",
                columns: table => new
                {
                    id_prijave = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_prijavljena_osoba = table.Column<int>(type: "int", nullable: false),
                    id_prijavljaca = table.Column<int>(type: "int", nullable: false),
                    razlog = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    admin_komentar = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    kreirano = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    obradjeno = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prijave", x => x.id_prijave);
                    table.ForeignKey(
                        name: "FK_prijave_klijenti_id_prijavljaca",
                        column: x => x.id_prijavljaca,
                        principalTable: "klijenti",
                        principalColumn: "id_klijent",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prijave_korisnici_id_prijavljena_osoba",
                        column: x => x.id_prijavljena_osoba,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "razgovor",
                columns: table => new
                {
                    id_razgovora = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_klijent = table.Column<int>(type: "int", nullable: false),
                    id_majstor = table.Column<int>(type: "int", nullable: false),
                    korisniciid_korisnik = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_razgovor", x => x.id_razgovora);
                    table.ForeignKey(
                        name: "FK_razgovor_klijenti_id_klijent",
                        column: x => x.id_klijent,
                        principalTable: "klijenti",
                        principalColumn: "id_klijent",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_razgovor_korisnici_id_majstor",
                        column: x => x.id_majstor,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_razgovor_korisnici_korisniciid_korisnik",
                        column: x => x.korisniciid_korisnik,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "zahtevi",
                columns: table => new
                {
                    id_zahteva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_klijenta = table.Column<int>(type: "int", nullable: false),
                    id_izvodjaca = table.Column<int>(type: "int", nullable: false),
                    datum = table.Column<DateOnly>(type: "date", nullable: false),
                    vreme = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    opis_radova = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    adresa = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zahtevi", x => x.id_zahteva);
                    table.ForeignKey(
                        name: "FK_zahtevi_klijenti_id_klijenta",
                        column: x => x.id_klijenta,
                        principalTable: "klijenti",
                        principalColumn: "id_klijent",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_zahtevi_korisnici_id_izvodjaca",
                        column: x => x.id_izvodjaca,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "porukes",
                columns: table => new
                {
                    id_poruke = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_razgovora = table.Column<int>(type: "int", nullable: false),
                    posiljac_id = table.Column<int>(type: "int", nullable: false),
                    tekst = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    vreme = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_porukes", x => x.id_poruke);
                    table.ForeignKey(
                        name: "FK_porukes_korisnici_posiljac_id",
                        column: x => x.posiljac_id,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_porukes_razgovor_id_razgovora",
                        column: x => x.id_razgovora,
                        principalTable: "razgovor",
                        principalColumn: "id_razgovora",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "zakazivanje",
                columns: table => new
                {
                    id_zakazivanja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_zahteva = table.Column<int>(type: "int", nullable: false),
                    cena_donja = table.Column<int>(type: "int", nullable: false),
                    cena_gornja = table.Column<int>(type: "int", nullable: false),
                    konacna_cena = table.Column<int>(type: "int", nullable: false),
                    da_li_je_placeno = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    pocetak = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    kraj = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    datum = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    opis = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    otkazano_od = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    razlog_otkazivanja = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    klijentiid_klijent = table.Column<int>(type: "int", nullable: true),
                    korisniciid_korisnik = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zakazivanje", x => x.id_zakazivanja);
                    table.ForeignKey(
                        name: "FK_zakazivanje_klijenti_klijentiid_klijent",
                        column: x => x.klijentiid_klijent,
                        principalTable: "klijenti",
                        principalColumn: "id_klijent");
                    table.ForeignKey(
                        name: "FK_zakazivanje_korisnici_korisniciid_korisnik",
                        column: x => x.korisniciid_korisnik,
                        principalTable: "korisnici",
                        principalColumn: "id_korisnik");
                    table.ForeignKey(
                        name: "FK_zakazivanje_zahtevi_id_zahteva",
                        column: x => x.id_zahteva,
                        principalTable: "zahtevi",
                        principalColumn: "id_zahteva",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_firme_pib",
                table: "firme",
                column: "pib",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gradovi_naziv_grada",
                table: "gradovi",
                column: "naziv_grada",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gradovi_rada_id_grada",
                table: "gradovi_rada",
                column: "id_grada");

            migrationBuilder.CreateIndex(
                name: "IX_gradovi_rada_id_izvodjaca",
                table: "gradovi_rada",
                column: "id_izvodjaca");

            migrationBuilder.CreateIndex(
                name: "IX_kategorije_id_roditelja",
                table: "kategorije",
                column: "id_roditelja");

            migrationBuilder.CreateIndex(
                name: "IX_kategorije_naziv_kategorije",
                table: "kategorije",
                column: "naziv_kategorije",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_klijenti_gradGdeKlijentRadid_grad",
                table: "klijenti",
                column: "gradGdeKlijentRadid_grad");

            migrationBuilder.CreateIndex(
                name: "IX_klijenti_id_grad_rada",
                table: "klijenti",
                column: "id_grad_rada",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_korisnici_email",
                table: "korisnici",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_majstori_jmbg",
                table: "majstori",
                column: "jmbg",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_obavestenja_korisnik_id",
                table: "obavestenja",
                column: "korisnik_id");

            migrationBuilder.CreateIndex(
                name: "IX_obavestenja_povezani_id",
                table: "obavestenja",
                column: "povezani_id");

            migrationBuilder.CreateIndex(
                name: "IX_ocene_id_izvodjaca",
                table: "ocene",
                column: "id_izvodjaca");

            migrationBuilder.CreateIndex(
                name: "IX_ocene_id_klijenta",
                table: "ocene",
                column: "id_klijenta");

            migrationBuilder.CreateIndex(
                name: "IX_porukes_id_razgovora",
                table: "porukes",
                column: "id_razgovora");

            migrationBuilder.CreateIndex(
                name: "IX_porukes_posiljac_id",
                table: "porukes",
                column: "posiljac_id");

            migrationBuilder.CreateIndex(
                name: "IX_prijave_id_prijavljaca",
                table: "prijave",
                column: "id_prijavljaca");

            migrationBuilder.CreateIndex(
                name: "IX_prijave_id_prijavljena_osoba",
                table: "prijave",
                column: "id_prijavljena_osoba");

            migrationBuilder.CreateIndex(
                name: "IX_pripada_id_izvodjaca",
                table: "pripada",
                column: "id_izvodjaca");

            migrationBuilder.CreateIndex(
                name: "IX_pripada_id_kategorije",
                table: "pripada",
                column: "id_kategorije");

            migrationBuilder.CreateIndex(
                name: "IX_razgovor_id_klijent",
                table: "razgovor",
                column: "id_klijent");

            migrationBuilder.CreateIndex(
                name: "IX_razgovor_id_majstor",
                table: "razgovor",
                column: "id_majstor");

            migrationBuilder.CreateIndex(
                name: "IX_razgovor_korisniciid_korisnik",
                table: "razgovor",
                column: "korisniciid_korisnik");

            migrationBuilder.CreateIndex(
                name: "IX_zahtevi_id_izvodjaca",
                table: "zahtevi",
                column: "id_izvodjaca");

            migrationBuilder.CreateIndex(
                name: "IX_zahtevi_id_klijenta",
                table: "zahtevi",
                column: "id_klijenta");

            migrationBuilder.CreateIndex(
                name: "IX_zakazivanje_id_zahteva",
                table: "zakazivanje",
                column: "id_zahteva");

            migrationBuilder.CreateIndex(
                name: "IX_zakazivanje_klijentiid_klijent",
                table: "zakazivanje",
                column: "klijentiid_klijent");

            migrationBuilder.CreateIndex(
                name: "IX_zakazivanje_korisniciid_korisnik",
                table: "zakazivanje",
                column: "korisniciid_korisnik");

            migrationBuilder.CreateIndex(
                name: "IX_zapisnik_admin_id_admina",
                table: "zapisnik_admin",
                column: "id_admina");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "firme");

            migrationBuilder.DropTable(
                name: "gradovi_rada");

            migrationBuilder.DropTable(
                name: "majstori");

            migrationBuilder.DropTable(
                name: "obavestenja");

            migrationBuilder.DropTable(
                name: "ocene");

            migrationBuilder.DropTable(
                name: "porukes");

            migrationBuilder.DropTable(
                name: "prijave");

            migrationBuilder.DropTable(
                name: "pripada");

            migrationBuilder.DropTable(
                name: "zakazivanje");

            migrationBuilder.DropTable(
                name: "zapisnik_admin");

            migrationBuilder.DropTable(
                name: "razgovor");

            migrationBuilder.DropTable(
                name: "kategorije");

            migrationBuilder.DropTable(
                name: "zahtevi");

            migrationBuilder.DropTable(
                name: "klijenti");

            migrationBuilder.DropTable(
                name: "gradovi");

            migrationBuilder.DropTable(
                name: "korisnici");
        }
    }
}
