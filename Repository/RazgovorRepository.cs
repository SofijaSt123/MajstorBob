using Majstor_bob.Data;
using Majstor_bob.Dto;
using Majstor_bob.Enums;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using System.Xml.Linq;

namespace Majstor_bob.Repository
{
    public class RazgovorRepository : IRazgovorRepository
    {
        private readonly AppDbContext _context;
        public RazgovorRepository(AppDbContext context)
        {
            _context = context;
        }
        public bool CreateRazgovor(razgovor razgovor)
        {
            _context.Add(razgovor);
            return Save();
        }

        public bool DeleteRazgovor(razgovor razgovor)
        {
            _context.Remove(razgovor);
            return Save();
        }

        public razgovor GetRazgovorId(int id)
        {
            return _context.razgovor.Where(r => r.id_razgovora == id).FirstOrDefault();
        }

        public razgovor GetRazgovorByIds(int idUser1, int idUser2)
        {
            return _context.razgovor.Where(r =>
                    (r.id_klijent == idUser1 && r.id_majstor == idUser2) ||
                    (r.id_klijent == idUser2 && r.id_majstor == idUser1)).FirstOrDefault();
        }
        public bool RazgovorExists(int id)
        {
            return _context.razgovor.Any(r=>r.id_razgovora==id);
        }

        public bool RazgovorExists(int idUser1, int idUser2)
        {
            return _context.razgovor.Any(r =>
                    (r.id_klijent == idUser1 && r.id_majstor == idUser2) ||
                    (r.id_klijent == idUser2 && r.id_majstor == idUser1));
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false; 
        }

        public bool UpdateRazgovor(razgovor razgovor)
        {
            _context.Update(razgovor);
            return Save();
        }

        public ICollection<GetRazgovorDto> GetRazgovoreByUserId(int userId,Tip_korisnika tip)
        {
            if (tip == Tip_korisnika.Firma)
            {
                return _context.razgovor
                    .Where(r => r.id_majstor == userId)
                    .Select(r => new GetRazgovorDto
                    {
                        id_razgovora = r.id_razgovora,
                        name = r.klijent.korisnik.ime, 
                    })
                    .ToList();
            }else
            if (tip == Tip_korisnika.Majstor)
            {
                return _context.razgovor
                    .Where(r => r.id_majstor == userId)
                    .Select(r => new GetRazgovorDto
                    {
                        id_razgovora = r.id_razgovora,
                        name = r.klijent.korisnik.ime,
                    })
                    .ToList();
            }
            
            return _context.razgovor
            .Where(r => r.id_klijent == userId)
            .Select(r => new GetRazgovorDto
            {
                id_razgovora = r.id_razgovora,
                name = r.izvodjac.ime,
            })
            .ToList();

        }
           
        

    }
}
