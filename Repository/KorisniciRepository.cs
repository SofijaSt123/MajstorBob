using Majstor_bob.Data;
using Majstor_bob.Enums;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Majstor_bob.Repository
{
    public class KorisniciRepository : IKorisniciRepository
    {
        private readonly AppDbContext _context;
        public KorisniciRepository(AppDbContext context) 
        {
            _context = context;
        }

        public bool CreateKorisnik(korisnici korisnik)
        { 
                _context.korisnici.Add(korisnik);
                return Save(); ;
        }

        public bool DeleteKorisnik(korisnici korisnik)
        {
            _context.Remove(korisnik);
            return Save();
        }

        public bool KorisnikExists(int id)
        {
            return _context.korisnici.Any(k=>k.id_korisnik==id);
        }

        public korisnici GetByEmail(string email)
        {
           return _context.korisnici.Where(k=>k.email ==email).FirstOrDefault(); //moze i _context.korisnici.FirstOrDefault(k=>k.email==email)
        }

        public korisnici GetKorisnikId(int id)
        {
            return _context.korisnici.Find(id); //Ovo je samo za pk
        }

        public ICollection<korisnici> GetKorisnicis()
        {
            return _context.korisnici.OrderBy(p => p.id_korisnik).ToList();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;

        }

        public bool UpdateKorisnik(korisnici korisnik)
        {
            _context.Update(korisnik);
            return Save();

        }

        public bool KorisnikExists(string email, string lozinka)
        {
            return _context.korisnici.Any(k=>k.email==email && k.lozinka==lozinka);
        }

        public ICollection<korisnici> GetByTip(Tip_korisnika tip)
        {
            var korisnici= _context.korisnici.Where(k=>k.tip_korisnika==tip).ToList();
            return korisnici;
        }

        public ICollection<korisnici> GetIzvodjaci()
        {
            var korisnici =_context.korisnici.Where(k =>
            k.tip_korisnika == Tip_korisnika.Majstor ||
            k.tip_korisnika == Tip_korisnika.Firma).ToList();
            
            return korisnici;
        }
    }
}
/*
 public decimal GetPokemonRating(int pokeId)
        {
            var review = _context.Reviews.Where(p => p.Pokemon.Id == pokeId);

            if (review.Count() <= 0)
                return 0;

            return ((decimal)review.Sum(r => r.Rating) / review.Count());
        }
*/