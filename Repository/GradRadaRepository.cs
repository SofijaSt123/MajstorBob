using Majstor_bob.Data;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Microsoft.EntityFrameworkCore;

namespace Majstor_bob.Repository
{
    public class GradRadaRepository : IGradoviradaRepository
    {
        private readonly AppDbContext _context;
        public GradRadaRepository(AppDbContext context)
        {
            _context = context;
        }
        public bool CreateGradRada(gradovi_rada grad)
        {
            _context.Add(grad);
            return Save();
        }

        public bool DeleteGradRada(gradovi_rada grad)
        {
            _context.Remove(grad);
            return Save();
        }

        public ICollection<gradovi_rada> GetGradovirada()
        {
            return _context.gradovi_rada.OrderBy(g=>g.id_grada).ToList();
        }

        public gradovi_rada GetGradRadaId(int id)
        {
            return _context.gradovi_rada.Find(id);
        }

        public ICollection<korisnici> GetIzvodjacByGrad(int gradId,bool doplata)
        {
            var ids = _context.gradovi_rada
                .Where(g => g.id_grada == gradId && g.zona_rada == doplata)
                .Select(g => g.id_izvodjaca)
                .ToList();

            return _context.korisnici
                .Where(k => ids.Contains(k.id_korisnik))
                .Include(k => k.kategorija_kojoj_pripada)
                .Include(k => k.majstori)
                .Include(k => k.firme)
                .ToList();
        }

        public bool GradRadaExists(int id)
        {
            return _context.gradovi_rada.Any(g=>g.id_grad_rada==id);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

            public void UpdateGradRada(gradovi_rada grad)
        {
            throw new NotImplementedException();
        }

        public ICollection<gradovi_rada> GetGradoveIzvodjaca(int id_izvodjaca)
        {
            return _context.gradovi_rada.Where(g => g.id_izvodjaca == id_izvodjaca).ToList();
        }



    }
}
