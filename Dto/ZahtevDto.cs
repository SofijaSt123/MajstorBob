using Majstor_bob.Models;

namespace Majstor_bob.Dto
{
    public class ZahtevDto
    { 
        public int id_izvodjaca { get; set; }
        public DateOnly datum { get; set; }
        public TimeOnly vreme { get; set; }
        public string opis_radova { get; set; }
        public string adresa { get; set; }
    }
}
