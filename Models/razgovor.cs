namespace Majstor_bob.Models
{
    public class razgovor
    {
        public int id_razgovora { get; set; }
        public int id_klijent { get; set; }
        public int id_majstor { get; set; }

        public klijenti klijent {get; set;}
        public korisnici izvodjac { get; set;}
        public ICollection<poruke> poruke {get; set;}   


    }
}
