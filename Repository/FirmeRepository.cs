using Majstor_bob.Data;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using static Majstor_bob.Helper.EncryptionHelp;

namespace Majstor_bob.Repository
{
    public class FirmeRepository : IFirmeRepository
    {
        private readonly AppDbContext _context;
        public FirmeRepository(AppDbContext context)
        {
            _context = context;
        }
        public bool CreateFirmu(firme firma)
        {
            _context.Add(firma);
            return Save();
        }

        public bool DeleteFirmu(int id)
        {
            throw new NotImplementedException();
        }

        public bool ExistPib(string pib)
        {
            var hash = HashHelper.Sha256(pib);

            return _context.firme.Any(x => x.pib == hash);
        }

        public bool FirmaExists(int id)
        {
            return _context.firme.Any(f=>f.id_firme==id);
        }

        public ICollection<firme> GetFirme()
        {
            return _context.firme.OrderBy(f=>f.id_firme).ToList();
        }

        public firme GetFirmuId(int id)
        {
            return _context.firme.Where(f=>f.id_firme==id).FirstOrDefault();
        }

        public bool Save()
        {
            var save = _context.SaveChanges();
            return save >0 ? true : false;
        }

        public bool UpdateFirmu(firme firma)
        {
            _context.Update(firma);
            return Save();
        }
    }
}
