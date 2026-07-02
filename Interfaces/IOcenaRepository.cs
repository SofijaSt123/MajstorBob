using Majstor_bob.Models;

namespace Majstor_bob.Interfaces
{
    public interface IOcenaRepository
    {
        ICollection<ocene> GetOcene();
        ocene GetOcenaId(int id);
        bool CreateOcenu(ocene ocena);
        bool UpdateOcena(ocene ocena);
        bool DeleteOcenu(ocene ocena);
        bool OcenaExists(int id);
        bool Save();

        //+++
        ICollection<ocene> getIzvodjacOcene(int id);
       


    }
}
