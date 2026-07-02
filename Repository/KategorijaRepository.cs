using Majstor_bob.Data;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using System.Collections.Immutable;

namespace Majstor_bob.Repository
{
    public class KategorijaRepository : IKategorijeRepository
    {
        private AppDbContext _context;
        public KategorijaRepository(AppDbContext context)
        {
            _context = context;
        }
        public bool CatagoryExists(int id)
        {
            return _context.kategorije.Any(k => k.id_kategorije == id);
        }

        public bool CreateKategoriju(kategorije kategorija)
        {
            _context.Add(kategorija);
            return Save();
        }

        public bool DeleteKategoriju()
        {
            throw new NotImplementedException();
        }

        public ICollection<kategorije> GetKategorije()
        {
            return _context.kategorije.OrderBy(p => p.id_kategorije).ToList();
        }

        public kategorije GetKategoriju(int id)
        {
            return _context.kategorije.Where(k=>k.id_kategorije==id).FirstOrDefault();
        }

        public kategorije GetRoditeljKategoriju(int id)
        {
            var kategorija =_context.kategorije.FirstOrDefault(k => k.id_kategorije == id);
            if (kategorija == null || kategorija.id_roditelja == null)
                return null;
            return _context.kategorije.FirstOrDefault(k => k.id_kategorije == kategorija.id_roditelja);
        }

        public bool KategorijePostoje(List<int> id)
        {
           
            if (id.Count != id.Distinct().Count())
                return false;

            // 2. PROVERA DA SVI POSTOJE U BAZI
            var existingIds = _context.kategorije
                .Where(k => id.Contains(k.id_kategorije))
                .Select(k => k.id_kategorije)
                .ToList();

            return id.All(id => existingIds.Contains(id));
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }


        public ICollection<kategorije> GetIzvodjacKategorije(int id)
        {
            var katId = _context.pripada.Where(p => p.id_izvodjaca == id).Select(p => p.id_kategorije);

            return _context.kategorije.Where(k=>katId.Contains(k.id_kategorije)).ToList();
        }
    }
}
