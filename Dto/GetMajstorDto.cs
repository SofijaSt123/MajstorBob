using Majstor_bob.Enums;

namespace Majstor_bob.Dto
{
    public class GetMajstorDto
    {
        public string? stripe_num { get; set; }
        public string? profilna_slika { get; set; }
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

    }
}
