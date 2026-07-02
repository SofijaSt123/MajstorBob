using Majstor_bob.Enums;
using Majstor_bob.Models;

namespace Majstor_bob.Dto
{
    public class GetFirmaDto
    {
        public string? stripe_num { get; set; }
        public bool verifikovan { get; set; }
        public string email { get; set; }
        public string ime { get; set; }
        public string prezime { get; set; }
        public string? telefon { get; set; }
        public Tip_korisnika tip_korisnika { get; set; } //enum
        public DateTime datum_registacije { get; set; }
        public string? opis_usluga { get; set; }
        public TimeOnly pocetak_radnog_vremena { get; set; }
        public TimeOnly kraj_radnog_vremena { get; set; }
        public string? naziv_firme { get; set; }
        public int broj_dostupnih_radnika { get; set; } = 1;
        public int broj_ukupnih_radnika { get; set; } = 1;

        public string? profilna_slika { get; set; }
    }
}
