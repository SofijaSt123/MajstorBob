namespace Majstor_bob.Dto
{
    public class UpdateFirmeDto
    {
        public string ime { get; set; }
        public string prezime { get; set; }
        public string? telefon { get; set; }
        //public IFormFile? profilna_slika { get; set; }
        public string naziv_firme { get; set; }
        public int broj_dostupnih_radnika { get; set; }
        public int broj_ukupnih_radnika { get; set; }
        public string opis_usluga { get; set; }
        public TimeOnly pocetak_radnog_vremena { get; set; }
        public TimeOnly kraj_radnog_vremena { get; set; }
    }
}
