using Majstor_bob.Data;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Majstor_bob.Repository
{
    public class KlijentiRepository : IKlijentiRepository
    {
        private readonly AppDbContext _context;

        public KlijentiRepository(AppDbContext context)
        {
            _context=context;
        }

        public bool CreateKlijenti(klijenti klijent)
        {
            _context.Add(klijent);
            return Save();
        }

        public void DeleteKlijent(int id)
        {
            throw new NotImplementedException();
        }

        public klijenti GetKlijenti(int id)
        {
            return _context.klijenti.Where(k => k.id_klijent == id).FirstOrDefault();
        }


        public bool KlijentExists(int id)
        {
            return _context.klijenti.Any(k=>k.id_klijent==id);
        }

        public bool Save()
        {
            var save=_context.SaveChanges();
            return save >0 ? true : false;
        }


        bool IKlijentiRepository.UpdateKlijent(klijenti klijent)
        {
           _context.Update(klijent);
            return Save();
        }
    }
}
