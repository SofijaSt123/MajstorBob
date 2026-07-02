using Majstor_bob.Data;
using Majstor_bob.Dto;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using System.Collections.Frozen;

namespace Majstor_bob.Repository
{
    public class PripadaRepository : IPripadaRepository
    {
        private readonly AppDbContext _context;
        public PripadaRepository(AppDbContext context)
        {
            _context = context;
        }
        public bool CreatePripada(pripada pripada)
        {
            _context.Add(pripada);
            return Save();
        }

        public ICollection<kategorije> GetKategorijeZaIzvodjaca(int id)
        {
            return _context.pripada.Where(p => p.id_izvodjaca == id).Select(p => p.kategorija).ToList();
        }

        public bool Save()
        {
            var save = _context.SaveChanges();
            return save > 0 ? true : false;
        }

        public ICollection<korisnici> GetIzvodjaciZaKategoriju(int id,ICollection<korisnici> k)
        {
            var ids = k.Select(x => x.id_korisnik).ToList();

            return _context.pripada
                .Where(p => p.id_kategorije == id &&
                            ids.Contains(p.id_izvodjaca))
                .Select(p => p.izvodjac)
                .ToList();
        }

        public bool DeletePripadaIzvodjaca(int id_izvodjaca)
        {
            var transaction=_context.Database.BeginTransaction();
            try
            {
                var pripada = _context.pripada
                .Where(p => p.id_izvodjaca == id_izvodjaca)
                .ToList();

                _context.pripada.RemoveRange(pripada);
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

        public bool AddPripad(ICollection<pripada> pripada)
        {
            _context.pripada.AddRange(pripada);
            return Save();
        }

        public pripada GetPripada(int idi, int idk)
        {
            var pripada=_context.pripada.Where(p=>p.id_izvodjaca==idi && p.id_kategorije==idk).FirstOrDefault();
            return pripada;
        }

        public bool DeletePripada(pripada prip)
        {
        _context.Remove(prip);
            return Save();
        }
    }
}
