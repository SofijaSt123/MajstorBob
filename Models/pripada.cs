namespace Majstor_bob.Models
{
    public class pripada
    {
        public int id_pripada {  get; set; }
        public int id_izvodjaca { get; set; }
        public int id_kategorije { get; set; }

        public korisnici izvodjac {  get; set; } //1
        public kategorije kategorija { get; set; } //1


    }
}
