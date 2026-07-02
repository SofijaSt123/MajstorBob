namespace Majstor_bob.Models
{
    public class prijave
    {
        public int id_prijave { get; set; }
        public int id_prijavljena_osoba { get; set; }
        public int id_prijavljaca { get; set; }
        public string razlog { get; set; }
        public string? admin_komentar { get; set; }
        public DateTime kreirano { get; set; }
        public DateTime? obradjeno { get; set; }
        public korisnici izvodjac {  get; set; } //1
        public klijenti klijent {  get; set; } //1

    }
}
