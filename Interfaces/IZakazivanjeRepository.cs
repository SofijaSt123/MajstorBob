using Majstor_bob.Dto;
using Majstor_bob.Models;

namespace Majstor_bob.Interfaces
{
    public interface IZakazivanjeRepository
    { 
        zakazivanje GetZakazivanjeId(int id);

        bool CreateZakazivanje(zakazivanje zakaz);

        bool UpdateZakazivanje(zakazivanje zakaz);

        bool DeleteZakazivanje(zakazivanje zakaz);

        bool ZakazivanjeExists(int id);
        ICollection<ListaZakazivanjaDto> GetZakazivanjaKlijenta(int klijentId);

        ICollection<ListaZakazivanjaDto> GetZakazivanjaIzvodjaca(int izvodjacId);
        bool Save();
    }
}
