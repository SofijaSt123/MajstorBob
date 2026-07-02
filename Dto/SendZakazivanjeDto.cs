using Majstor_bob.Enums;
using Majstor_bob.Models;

namespace Majstor_bob.Dto
{
    public class SendZakazivanjeDto
    {
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
