using Majstor_bob.Enums;
using Majstor_bob.Models;
using Microsoft.AspNetCore.Identity; 

namespace Majstor_bob.Interfaces
{

    public interface IKorisniciRepository
    {
        ICollection<korisnici> GetKorisnicis();

        ICollection<korisnici> GetByTip(Tip_korisnika tip);

        ICollection<korisnici> GetIzvodjaci();
        korisnici GetKorisnikId(int id);
        korisnici GetByEmail(string email);
        bool CreateKorisnik(korisnici korisnik);
        bool UpdateKorisnik(korisnici korisnik);
        bool DeleteKorisnik(korisnici korisnik);
        bool KorisnikExists(int id);
        bool KorisnikExists(string email, string lozinka);
        bool Save();


    }
}
