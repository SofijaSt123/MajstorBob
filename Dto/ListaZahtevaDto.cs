using Majstor_bob.Enums;

namespace Majstor_bob.Dto
{
    public class ListaZahtevaDto
    {
        public int id_zahteva { get; set; }
        public int id_klijenta { get; set; }
        public int id_izvodjaca { get; set; }
        public DateOnly datum { get; set; }
        public TimeOnly vreme { get; set; }
        public string opis_radova { get; set; }
        public string adresa { get; set; }
        public Stanje_zahteva status { get; set; }
    }
}
