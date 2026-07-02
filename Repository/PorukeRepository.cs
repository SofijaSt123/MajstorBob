using Majstor_bob.Data;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;

namespace Majstor_bob.Repository
{
    public class PorukeRepository : IPorukeRepository
    {

        private readonly AppDbContext _context;
        public PorukeRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool CreatePoruke(poruke poruka)
        {
            _context.Add(poruka);
            return Save();
        }

        public bool DeletePoruke(poruke poruka)
        {
            _context.Remove(poruka);
            return Save();
        }

        public ICollection<poruke> GetPoruke(int idRazgovora)
        {
            return _context.porukes
                .Where(p => p.id_razgovora == idRazgovora)
                .OrderBy(p => p.vreme)
                .ToList();
        }

        public poruke GetPorukeId(int id)
        {
            return _context.porukes.Where(p=>p.id_poruke==id).FirstOrDefault();
        }

        public bool PorukeExists(int id)
        {
            return _context.porukes.Any(p=>p.id_poruke==id);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool UpdatePoruke(poruke poruka)
        {
            _context.Update(poruka);
            return Save();
        }
    }
}
