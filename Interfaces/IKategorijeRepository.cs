using Majstor_bob.Models;
using System.Collections.ObjectModel;

namespace Majstor_bob.Interfaces
{
    public interface IKategorijeRepository
    {
        ICollection<kategorije> GetKategorije();
        kategorije GetKategoriju(int id);
        kategorije GetRoditeljKategoriju(int id);
        bool CatagoryExists(int id);
        bool CreateKategoriju(kategorije kategorija);
        bool DeleteKategoriju();
        bool Save();

        ICollection<kategorije> GetIzvodjacKategorije(int id);
        bool KategorijePostoje(List<int> id);
    }
}
