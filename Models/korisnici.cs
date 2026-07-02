using Majstor_bob.Enums;

namespace Majstor_bob.Models
{
    public class korisnici
    {
        public int id_korisnik { get; set; }
        public string email { get; set; }
        public string lozinka { get; set; }
        public string ime { get; set; }
        public string prezime { get; set; }
        public string? telefon { get; set; }
        public byte[]? profilna_slika { get; set; }
        public Tip_korisnika tip_korisnika { get; set; } //enum
        public bool verifikovan { get; set; }
        public bool aktivan { get; set; }
        public DateTime datum_registacije { get; set; }
        public DateTime poslednji_login { get; set; }
        public string refresh_token { get; set; }
        public DateTime token_istice { get; set; }
        public bool blokiran { get; set; }
        public bool is_flaged { get; set; }
        public string? stripe_num { get; set; }

        public korisnici()
        {
            datum_registacije = DateTime.UtcNow;
            poslednji_login = DateTime.UtcNow;
            token_istice = DateTime.UtcNow.AddDays(7);

            refresh_token = "";
            telefon = "";

            verifikovan = false;
            aktivan = true;
            blokiran = false;
            is_flaged = false;
        }
        public ICollection<gradovi_rada> gradovi_rad { get; set; }
        public ICollection<obavestenja> obavestenje_primalac { get; set; }
        public ICollection<obavestenja> obavestenja_posiljac { get; set; }
        public ICollection<ocene> ocenjen_izvodjac { get; set; }
        public ICollection<prijave> prijavljena_osoba { get; set; }
        public ICollection<pripada> kategorija_kojoj_pripada { get; set; }
        public ICollection<zahtevi> kome_salje_zahtev { get; set; }
        public ICollection<zakazivanje> izvodjac_koj_zakazuje { get; set; }
        public ICollection<razgovor> razgovor { get; set; }
        public ICollection<zapisnik_admin> zapisnik {  get; set; }

        public majstori majstori { get; set; }
        public firme firme { get; set; }
        public klijenti klijenti { get; set; }
    }
}
