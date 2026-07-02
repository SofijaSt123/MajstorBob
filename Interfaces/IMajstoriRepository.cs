using Majstor_bob.Models;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Majstor_bob.Interfaces
{
    public interface IMajstoriRepository
    {
        ICollection<majstori> GetMajstori();
        majstori GetMajstorId(int id);
        bool CreateMajstor(majstori majstor);

        bool UpdateMajstor(majstori majstor);

        bool DeleteMajstor(majstori majstor);

        bool MajstorExists(int id);
        bool ExistJmbg(string jmbg);
        bool Save();
    }
}
