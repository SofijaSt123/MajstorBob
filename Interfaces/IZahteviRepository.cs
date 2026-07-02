using Majstor_bob.Dto;
using Majstor_bob.Enums;
using Majstor_bob.Models;

namespace Majstor_bob.Interfaces
{
    public interface IZahteviRepository
    {
        zahtevi GetZahtevById(int id);

        bool CreateZahtev(zahtevi zahtev);

        bool UpdateZahtev(zahtevi zahtev);

        bool DeleteZahtev(zahtevi zahtev);

        bool ZahtevExists(int id);
        ICollection<ListaZahtevaDto> GetZahteveIzvodjaca(int id);
        ICollection<ListaZahtevaDto> GetZahteveIzvodjacaByStanje(int id, Stanje_zahteva stanje);
        ICollection<ListaZahtevaDto> GetZahteveKlijenta(int id);
        bool Save();
    }
}
