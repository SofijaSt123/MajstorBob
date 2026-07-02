using Majstor_bob.Enums;

namespace Majstor_bob.Models
{
    public class zakazivanje
    {
        public int id_zakazivanja { get; set; }

        public int id_zahteva { get; set; }
        public zahtevi zahtev { get; set; }


        public int cena_donja { get; set; }
        public int cena_gornja { get; set; }
        public int konacna_cena {  get; set; }
        public bool da_li_je_placeno { get; set; }
        public TimeOnly pocetak {  get; set; }
        public TimeOnly kraj {  get; set; }
        public DateOnly datum {  get; set; }
        public Status_zakazivanja status { get; set; }
        public string opis { get; set; }
        public string? otkazano_od {  get; set; }
        public string? razlog_otkazivanja { get; set; }

        
    }
}
