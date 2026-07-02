using Majstor_bob.Data;
using Majstor_bob.Dto;
using Majstor_bob.Enums;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;

namespace Majstor_bob.Repository
{
    public class ZahtevRepository : IZahteviRepository
    {
        private readonly AppDbContext _context;

        public ZahtevRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool CreateZahtev(zahtevi zahtev)
        {
            _context.Add(zahtev);
            return Save();
        }

        public bool DeleteZahtev(zahtevi zahtev)
        {
            _context.Remove(zahtev);
            return Save();
        }

        public zahtevi GetZahtevById(int id)
        {
            return _context.zahtevi.Where(z=>z.id_zahteva==id).FirstOrDefault();
        }

        public ICollection<ListaZahtevaDto> GetZahteveIzvodjaca(int id)
        {
            return _context.zahtevi.Where(z => z.id_izvodjaca == id)
                .Select(z => new ListaZahtevaDto
                {
                    id_zahteva=z.id_zahteva,
                    id_klijenta=z.id_klijenta,
                    adresa = z.adresa,
                    datum = z.datum,
                    opis_radova = z.opis_radova,
                    vreme = z.vreme,
                    status=z.status,
                    id_izvodjaca=z.id_izvodjaca,
                    
                }).OrderBy(z => z.datum).ToList();         
        }


        public ICollection<ListaZahtevaDto> GetZahteveIzvodjacaByStanje(int id,Stanje_zahteva stanje)
        {
            return _context.zahtevi.Where(z => z.id_izvodjaca == id && z.status==stanje)
                .Select(z => new ListaZahtevaDto
                {
                   id_klijenta = z.id_klijenta,
                    adresa = z.adresa,
                    datum = z.datum,
                    opis_radova = z.opis_radova,
                    vreme = z.vreme,
                    status = z.status,

                }).OrderBy(z => z.datum).ToList();
        }

        public ICollection<ListaZahtevaDto> GetZahteveKlijenta(int id)
        {
            return _context.zahtevi.Where(z => z.id_klijenta == id)
                .Select(z => new ListaZahtevaDto
                {
                    id_klijenta = z.id_klijenta,
                    id_izvodjaca=z.id_izvodjaca,
                    adresa = z.adresa,
                    datum = z.datum,
                    opis_radova = z.opis_radova,
                    vreme = z.vreme,
                    status = z.status,
                    id_zahteva = z.id_zahteva,

                }).OrderBy(z => z.datum).ToList();
        }



        public zahtevi GetZahtevByIdand(int id, Stanje_zahteva stanje)
        {
            return _context.zahtevi.Where(z => z.id_zahteva == id).FirstOrDefault();
        }



        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool UpdateZahtev(zahtevi zahtev)
        {
            _context.Update(zahtev);
            return Save();
        }

        public bool ZahtevExists(int id)
        {
            return _context.zahtevi.Any(z => z.id_zahteva == id);
        }
    }
}
