using Majstor_bob.Data;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;

namespace Majstor_bob.Repository
{
    public class PrijaveRepository : IPrijaveRepository
    {
        private readonly AppDbContext _context;
        public PrijaveRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool CreatePrijavu(prijave prijava)
        {
            _context.Add(prijava);
            return Save();
        }

        public bool DeletePrijava(prijave prijava)
        {
            _context.Remove(prijava);
            return Save();
        }

        public ICollection<prijave> GetPrijave()
        {
            return _context.prijave.Where(p=>p.obradjeno!=null).ToList();
        }

        public prijave GetPrijaveId(int id)
        {
        return _context.prijave.Find(id);
        }

        public bool PrijavaExists(int id)
        {
            throw new NotImplementedException();
        }

        public bool PrijavaiExists(int id)
        {
            return _context.prijave.Any(p => p.id_prijave == id);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool UpdatePrijava(prijave prijava)
        {
            _context.Update(prijava);
            return Save();
        }

        public bool VecPrijavio(int id_korisnik, int id_izvodjac)
        {
            return _context.prijave.Any(p=>p.id_prijavljaca==id_korisnik && p.id_prijavljena_osoba==id_izvodjac);
        }

        public ICollection<prijave> GetByKlijent(int id_klijent)
        {
            return _context.prijave
                .Where(p => p.id_prijavljaca == id_klijent)
                .ToList();
        }

        public ICollection<prijave> GetNeobradjene()
        {
            return _context.prijave
                .Where(p => p.obradjeno==null)
                .ToList();
        }

    }
}
