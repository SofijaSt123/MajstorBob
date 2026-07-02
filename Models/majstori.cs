using System.ComponentModel.DataAnnotations;

namespace Majstor_bob.Models
{
    public class majstori
    {
        [Key]
        public int id_majstora { get; set; }
        public korisnici korisnik { get; set; }
        public string? opis_usluga { get; set; }
        public TimeOnly pocetak_radnog_vremena { get; set; }
        public TimeOnly kraj_radnog_vremena { get; set; }
        public string? jmbg {  get; set; }
        public decimal prosek_ocena { get; set; }
        public int broj_recenzija { get; set; }



    }
}
