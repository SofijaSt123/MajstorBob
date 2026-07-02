namespace Majstor_bob.Dto
{
    public class IzvodjacDto
    {
        public string opis_usluga { get; set; }
        public TimeOnly pocetak_radnog_vremena { get; set; }
        public TimeOnly kraj_radnog_vremena { get; set; }
        public decimal prosek_ocena { get; set; }
        public int broj_recenzija { get; set; }

        //
        public string? ime {  get; set; }
        public string? naziv_firme { get; set; }
        public int? broj_dostupnih_radnika { get; set; }

    }
}
