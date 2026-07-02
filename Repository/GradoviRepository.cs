
using Majstor_bob.Data;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using System.Diagnostics.Metrics;

namespace Majstor_bob.Repository
{
    public class GradoviRepository : IGradoviRepository
    {

        private readonly AppDbContext _context;
        public GradoviRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool CreateGrad(gradovi grad)
        {
            _context.Add(grad);
            return Save();
        }

        public bool DeleteGrad(gradovi grad)
        {
            _context.Remove(grad);
            return Save();
        }

        public ICollection<gradovi> GetGradovi()
        {
            return _context.gradovi.OrderBy(g=>g.naziv_grada).ToList();
        }

        public gradovi GetGradoviId(int id)
        {
            return _context.gradovi.Where(g => g.id_grad == id).FirstOrDefault();
        }

        public bool GradExists(int id)
        {
            return _context.gradovi.Any(g => g.id_grad == id);
        }

        public bool GradExistsNaziv(string ime)
        {
            return _context.gradovi
                  .Any(g => g.naziv_grada.ToLower() == ime.ToLower());
        }

        public bool PrvoSlovoVeliko(string tekst)
        {
            return !string.IsNullOrEmpty(tekst) && char.IsUpper(tekst[0]);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;

        }

        public void UpdateGrad(gradovi grad)
        {
            throw new NotImplementedException();
        }
    }
}
