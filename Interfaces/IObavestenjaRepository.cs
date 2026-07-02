using Majstor_bob.Models;
using Microsoft.EntityFrameworkCore;

namespace Majstor_bob.Interfaces
{
    public interface IObavestenjaRepository
    {
       
        obavestenja GetObavestenjaId(int id);

        ICollection<obavestenja> GetObavestenjaKorisnika(int id);
        bool CreateObavestenje(obavestenja obbavestenje);

        bool UpdateObavestenje(obavestenja obavestenje);

        bool DeleteObavestenje(obavestenja obavestenja);

        bool ObavestenjeExists(int id);

        bool Save();

        //+
        public bool CreateObavestenja(ICollection<obavestenja> obavestenja);

        public bool UpdateObavestenja(ICollection<obavestenja> obavestenja);


        
    }
}
