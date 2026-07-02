using Majstor_bob.Models;

namespace Majstor_bob.Interfaces
{
    public interface IPrijaveRepository
    {
        ICollection<prijave> GetPrijave();
        prijave GetPrijaveId(int id);
        bool CreatePrijavu(prijave prijava);
        bool UpdatePrijava(prijave prijava);
        bool DeletePrijava(prijave prijava);
        bool PrijavaExists(int id);

        bool VecPrijavio(int id_korisnik,int id_izvodjac);
        bool Save();

        ICollection<prijave> GetByKlijent(int id_klijent);

        ICollection<prijave> GetNeobradjene();

    }
}
