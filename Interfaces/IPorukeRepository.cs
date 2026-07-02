
using Majstor_bob.Models;

namespace Majstor_bob.Interfaces
{
    public interface IPorukeRepository
    {
        ICollection<poruke> GetPoruke(int idRazgovora); //id klijent id izvodjac

        poruke GetPorukeId(int id);
        bool CreatePoruke(poruke poruka);

        bool UpdatePoruke(poruke poruka);

        bool DeletePoruke(poruke poruka);

        bool PorukeExists(int id);

        bool Save();
    }
}
