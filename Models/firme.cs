using System.ComponentModel.DataAnnotations;

namespace Majstor_bob.Models
{
    public class firme
    {
        [Key]
        public int id_firme { get; set; }
        public korisnici korisnik { get; set; }
        public string? naziv_firme { get; set; }
        public int broj_dostupnih_radnika { get; set; } = 1;
        public int broj_ukupnih_radnika { get; set; } = 1;
        public string? opis_usluga { get; set; }
        public TimeOnly pocetak_radnog_vremena { get; set; }
        public TimeOnly kraj_radnog_vremena { get; set; }
        public string? pib { get; set; }
        public decimal prosek_ocena { get; set; }
        public int broj_recenzija { get; set; }

    }
}
