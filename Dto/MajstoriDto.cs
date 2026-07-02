namespace Majstor_bob.Dto
{
    public class MajstoriDto 
    {
        public string ime { get; set; }
        public int id_majstora { get; set; }
        
        public string opis_usluga { get; set; }
        public TimeOnly pocetak_radnog_vremena { get; set; }
        public TimeOnly kraj_radnog_vremena { get; set; }
        public decimal prosek_ocena { get; set; }
        public int broj_recenzija { get; set; }
        
    }
}
