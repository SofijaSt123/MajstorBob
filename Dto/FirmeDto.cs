
namespace Majstor_bob.Dto
{
    public class FirmeDto 
    {
        public int id_firme { get; set; }
        public string naziv_firme { get; set; }
        public int broj_dostupnih_radnika { get; set; }
        public int broj_ukupnih_radnika { get; set; }

        
        public string opis_usluga { get; set; }
        public TimeOnly pocetak_radnog_vremena { get; set; }
        public TimeOnly kraj_radnog_vremena { get; set; }
        public decimal prosek_ocena { get; set; }
        public int broj_recenzija { get; set; }
        
    }
}
