using Majstor_bob.Models;

namespace Majstor_bob.Interfaces
{
    public interface IKlijentiRepository
    {
        klijenti GetKlijenti(int id);

        bool CreateKlijenti(klijenti klijent);
        bool UpdateKlijent(klijenti klijent);
        void DeleteKlijent(int id);
        bool KlijentExists(int id);

        bool Save();

    }
}
