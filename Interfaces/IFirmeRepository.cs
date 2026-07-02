using Majstor_bob.Models;

namespace Majstor_bob.Interfaces
{
    public interface IFirmeRepository
    {
        ICollection<firme> GetFirme();
        firme GetFirmuId(int id);
        bool CreateFirmu(firme firma);

        bool UpdateFirmu(firme firma);

        bool DeleteFirmu(int id);

        bool FirmaExists(int id);
        bool ExistPib(string pib);
        bool Save();

    }
}
