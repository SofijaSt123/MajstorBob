using Majstor_bob.Data;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;

namespace Majstor_bob.Repository
{
    public class OcenaRepository : IOcenaRepository
    {
        private readonly AppDbContext _context;
        public OcenaRepository(AppDbContext context)
        {
            _context = context;
        }
        public bool CreateOcenu(ocene ocena)
        {
            _context.Add(ocena);
            return Save();
        }

        public bool DeleteOcenu(ocene ocena)
        {
            _context.Remove(ocena);
            return Save();
        }

        public ICollection<ocene> getIzvodjacOcene(int id)
        {
            return _context.ocene
                  .Where(o=>o.id_izvodjaca==id)
                  .ToList();
        }

        public ocene GetOcenaId(int id)
        {
            return _context.ocene.Where(o=>o.id_ocene==id).FirstOrDefault();
        }

        public ICollection<ocene> GetOcene()
        {
            return _context.ocene.OrderBy(o=>o.id_ocene).ToList();
        }

        public bool OcenaExists(int id)
        {
            return _context.ocene.Any(o=>o.id_ocene==id);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;

        }

        public bool UpdateOcena(ocene ocena)
        {
            _context.Update(ocena);
            return Save();
        }
    }
}
