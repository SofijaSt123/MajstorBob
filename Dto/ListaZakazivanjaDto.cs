using Majstor_bob.Enums;

namespace Majstor_bob.Dto
{
    public class ListaZakazivanjaDto
    {
        public int? konacna_cena { get; set; }
        public bool da_li_je_placeno { get; set; }
        public int id_zakazivanja { get; set; }
        public int id_zahteva { get; set; }
        public int cena_donja { get; set; }
        public int cena_gornja { get; set; }
        public TimeOnly pocetak { get; set; }
        public TimeOnly kraj { get; set; }
        public DateOnly datum { get; set; }
        public Status_zakazivanja status { get; set; }
        public string opis { get; set; }
    }
}
