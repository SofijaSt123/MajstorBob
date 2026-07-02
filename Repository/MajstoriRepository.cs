using Majstor_bob.Data;
using Majstor_bob.Helper;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Microsoft.EntityFrameworkCore.Query.Internal;
using static Majstor_bob.Helper.EncryptionHelp;

namespace Majstor_bob.Repository
{
    public class MajstoriRepository : IMajstoriRepository
    {
        private readonly AppDbContext _context;
        public MajstoriRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool CreateMajstor(majstori majstor)
        {
            _context.Add(majstor);
            return Save();
        }

        public bool DeleteMajstor(majstori majstor)
        {
            _context.Remove(majstor);
            return Save();
        }

        public bool ExistJmbg(string jmbg)
        {
            var hash = HashHelper.Sha256(jmbg);

            return _context.majstori.Any(x => x.jmbg == hash);
        }

        public ICollection<majstori> GetMajstori()
        {
            return _context.majstori.OrderBy(m=>m.id_majstora).ToList();
        }

        public majstori GetMajstorId(int id)
        {
            return _context.majstori.Where(m=>m.id_majstora==id).FirstOrDefault();
        }

        public bool MajstorExists(int id)
        {
            return _context.majstori.Any(m=>m.id_majstora==id);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool UpdateMajstor(majstori majstor)
        {
            _context.Update(majstor);
                return Save();
        }
    }
}
