namespace Majstor_bob.Models
{
    public class ocene
    { 
        public int id_ocene { get; set; }
        public int id_klijenta { get; set; }
        public int id_izvodjaca { get; set; }
        public string? opis_recenzije { get; set; }
        public string? odgovor {  get; set; }
        public int ocena_cena { get; set; }
        public int ocena_kvaliteta { get; set; }
        public int ocena_brzine { get; set; }
        public int ocena_odnosa { get; set; }

        public klijenti klijent {  get; set; } //1
        public korisnici izvodjac {  get; set; } //1


    }
}
