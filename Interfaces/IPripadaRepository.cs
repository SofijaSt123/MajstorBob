using Majstor_bob.Dto;
using Majstor_bob.Models;

namespace Majstor_bob.Interfaces
{
    public interface IPripadaRepository
    {
        ICollection<kategorije> GetKategorijeZaIzvodjaca(int id);
        ICollection<korisnici> GetIzvodjaciZaKategoriju(int id,ICollection<korisnici> k);
        bool CreatePripada(pripada pripada);
        bool Save();

        bool DeletePripada(pripada prip);
        pripada GetPripada(int idi, int idk);
        bool AddPripad(ICollection<pripada> pripada);
        bool DeletePripadaIzvodjaca(int id_izvodjaca);

    }
}
