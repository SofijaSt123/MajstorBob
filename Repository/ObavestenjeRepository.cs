using Majstor_bob.Data;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;

namespace Majstor_bob.Repository
{
    public class ObavestenjeRepository : IObavestenjaRepository
    {

        private readonly AppDbContext _context;
        public ObavestenjeRepository(AppDbContext context)
        {
            _context = context;
        }
        public bool CreateObavestenje(obavestenja obbavestenje)
        {
            _context.Add(obbavestenje);
            return Save();
        }

        public bool DeleteObavestenje(obavestenja obavestenje)
        {
            _context.Remove(obavestenje);
            return Save();
        }

        public obavestenja GetObavestenjaId(int id)
        {
            return _context.obavestenja.Where(o=>o.id==id).FirstOrDefault();
        }

        public bool ObavestenjeExists(int id)
        {
            return _context.obavestenja.Any(o=>o.id==id);
        }

        public bool Save()
        {
            var save = _context.SaveChanges();
            return save >0 ? true : false;
        }

        public bool UpdateObavestenje(obavestenja obavestenje)
        {
            _context.Update(obavestenje);
            return Save();
        }

        public bool CreateObavestenja(ICollection<obavestenja> obavestenja)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                _context.obavestenja.AddRange(obavestenja);

                _context.SaveChanges();

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return false;
            }
        }

        public ICollection<obavestenja> GetObavestenjaKorisnika(int id)
        {
            return _context.obavestenja.Where(o=>o.receiver_id==id && o.procitanno==false).ToList();
        }

        public bool UpdateObavestenja(ICollection<obavestenja> obavestenja)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                _context.obavestenja.UpdateRange(obavestenja);

                _context.SaveChanges();

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return false;
            }
        }
    }
}
