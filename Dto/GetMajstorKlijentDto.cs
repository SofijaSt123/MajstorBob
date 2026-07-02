using Majstor_bob.Enums;

namespace Majstor_bob.Dto
{
    public class GetMajstorKijentDto
    {
        public string? stripe_num { get; set; }
        public bool verifikovan { get; set; }
        public string ime { get; set; }
        public string prezime { get; set; }
        public Tip_korisnika tip_korisnika { get; set; } //enum    }
        public string? telefon { get; set; }
        public string opis_usluga { get; set; }
        public TimeOnly pocetak_radnog_vremena { get; set; }
        public TimeOnly kraj_radnog_vremena { get; set; }
        public decimal prosek_ocena { get; set; }
        public int broj_recenzija { get; set; }
        public string? profilna_slika { get; set; }

    }
}
