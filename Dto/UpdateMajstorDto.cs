namespace Majstor_bob.Dto
{
    public class UpdateMajstorDto
    {
        public string ime { get; set; }
        public string prezime { get; set; }
        public string? telefon { get; set; }
        //public IFormFile? profilna_slika { get; set; }
        public string opis_usluga { get; set; }
        public TimeOnly pocetak_radnog_vremena { get; set; }
        public TimeOnly kraj_radnog_vremena { get; set; }
    }
}
