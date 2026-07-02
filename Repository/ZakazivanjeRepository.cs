using Majstor_bob.Data;
using Majstor_bob.Dto;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Microsoft.EntityFrameworkCore;

namespace Majstor_bob.Repository
{
    public class ZakazivanjeRepository : IZakazivanjeRepository
    {
        private readonly AppDbContext _context;
        public ZakazivanjeRepository(AppDbContext context)
        {
            _context = context;
        }
        public bool CreateZakazivanje(zakazivanje zakaz)
        {
            _context.Add(zakaz);
            return Save();
        }

        public bool DeleteZakazivanje(zakazivanje zakaz)
        {
            _context.Remove(zakaz);
            return Save();
        }

        public ICollection<ListaZakazivanjaDto> GetZakazivanjaIzvodjaca(int izvodjacId)
        {
            return _context.zakazivanje
     .Where(z => z.zahtev.id_izvodjaca == izvodjacId)
     .Select(z => new ListaZakazivanjaDto
     {
         da_li_je_placeno=z.da_li_je_placeno,
         konacna_cena=z.konacna_cena,
         id_zakazivanja=z.id_zakazivanja,
         id_zahteva=z.id_zahteva,
         datum = z.datum,
         pocetak = z.pocetak,
         kraj = z.kraj,
         cena_donja = z.cena_donja,
         cena_gornja = z.cena_gornja,
         opis = z.opis,
         status = z.status

     })
     .ToList();
        }

        public ICollection<ListaZakazivanjaDto> GetZakazivanjaKlijenta(int klijentId)
        {
            return _context.zakazivanje
         .Where(z => z.zahtev.id_klijenta == klijentId)
         .Select(z => new ListaZakazivanjaDto
         {
             id_zahteva=z.id_zahteva,
             id_zakazivanja=z.id_zakazivanja,
             datum = z.datum,
             pocetak = z.pocetak,
             da_li_je_placeno=z.da_li_je_placeno,
             konacna_cena=z.konacna_cena,
             kraj = z.kraj,
             cena_donja = z.cena_donja,
             cena_gornja = z.cena_gornja,
             opis = z.opis,
             status=z.status
             
         })
         .ToList();
        }


        public zakazivanje GetZakazivanjeId(int id)
        {
            var zakaz=_context.zakazivanje
                       .Include(z => z.zahtev)
                       .FirstOrDefault(z => z.id_zakazivanja == id);
            
           return zakaz;
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool UpdateZakazivanje(zakazivanje zakaz)
        {
            _context.Update(zakaz);
            return Save();
        }

        public bool ZakazivanjeExists(int id)
        {
            return _context.korisnici.Any(k => k.id_korisnik == id);
        }
    }
}
