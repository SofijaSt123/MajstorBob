using AutoMapper;
using Majstor_bob.Data;
using Majstor_bob.Dto;
using Majstor_bob.Enums;
using Majstor_bob.Helper;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Majstor_bob.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrijaveController : Controller
    {
        private readonly IPrijaveRepository _prijaveRepository;
        private readonly IMapper _mapper;
        private readonly IKorisniciRepository _korisniciRepository;
        private readonly IKlijentiRepository _klijentiRepository;
        private readonly IPripadaRepository _pripadaRepository;
        private readonly AppDbContext _context;
        public PrijaveController(IKorisniciRepository korisniciRepository, IKlijentiRepository klijentiRepository,
            IMapper mapper, IPrijaveRepository prijaveRepository, IPripadaRepository pripadaRepository
            , AppDbContext context)
        {
            _korisniciRepository = korisniciRepository;
            _klijentiRepository = klijentiRepository;
            _mapper = mapper;
            _prijaveRepository = prijaveRepository;
            _pripadaRepository = pripadaRepository;
            _context = context;
        }


        [Authorize]
        [HttpPost("CreatePrijavu")]
        public IActionResult CreatePrijavu([FromBody] CreatePrijaveDto dto,[FromQuery] int id_izvodjaca)
        {
            int id_klijenta;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_klijenta))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsKlijent(User))
                return StatusCode(403);

            if (_prijaveRepository.VecPrijavio(id_klijenta, id_izvodjaca))
                return BadRequest("Vec ste prijavili ovog korisnika");

            var izvodjac = _korisniciRepository.GetKorisnikId(id_izvodjaca);

            if ((izvodjac == null) || (izvodjac.tip_korisnika != Tip_korisnika.Majstor && izvodjac.tip_korisnika != Tip_korisnika.Firma))
                return BadRequest("Invalid user type");


            izvodjac.is_flaged = true;

            if(!_korisniciRepository.UpdateKorisnik(izvodjac))
                return BadRequest("Problem falaging user");

            var prijava = new prijave
            {
                id_prijavljaca = id_klijenta,
                id_prijavljena_osoba = id_izvodjaca,
                razlog = dto.razlog,
                kreirano = DateTime.Now,
                admin_komentar="Nije jos obradjeno"
            };

            if (!_prijaveRepository.CreatePrijavu(prijava))
                return BadRequest("Something went wrong creating prijava");

            return Ok("Prijava uspesno poslata");
        }

        [Authorize]
        [HttpGet("GetPrijave")]
        public IActionResult GetPrijave()
        {
            int id_user;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_user))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsAdminOrOwner(User,id_user.ToString()))
                return StatusCode(403);

            var korisnik=_korisniciRepository.GetKorisnikId(id_user);

            if (korisnik.tip_korisnika == Tip_korisnika.Klijent)
            {
                var prijave = _prijaveRepository.GetByKlijent(id_user);
                if (prijave == null || !prijave.Any())
                    return BadRequest("Nema prijava");

                var dto = _mapper.Map<List<GetPrijaveDto>>(prijave);
                return Ok(dto);
            }
            else if (korisnik.tip_korisnika == Tip_korisnika.Admin)
            {
                var prijave = _prijaveRepository.GetNeobradjene();
                if (prijave == null || !prijave.Any())
                    return BadRequest("Nema prijava");

                var dto = _mapper.Map<List<GetPrijaveDto>>(prijave);
                return Ok(dto);

            }else
            {
                return BadRequest("Unauthorized");
            }
        }

        [Authorize]
        [HttpPost("ObradiPrijavu")]
        public IActionResult ObradiPrijavu([FromBody] UpdatePrijavaDto dto,bool blokiraj)
        {
            int id_admin;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_admin))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsAdmin(User))
                return StatusCode(403);

            if(!ModelState.IsValid)
                return BadRequest(ModelState);


            var prijava = _prijaveRepository.GetPrijaveId(dto.id_prijave);

            
            if (prijava == null)
                return BadRequest("Nema prijave");
            if (prijava.obradjeno != null)
                return BadRequest("Prijava je već obrađena");


            var izvodjac = _korisniciRepository.GetKorisnikId(prijava.id_prijavljena_osoba);

            if (izvodjac == null)
                return BadRequest("Nema izvodjaca");

            var transaction=_context.Database.BeginTransaction();
            try
            {
                if (blokiraj)
                {
                    izvodjac.blokiran = true;
                    _context.korisnici.Update(izvodjac);

                    var pripada = _context.pripada
                        .Where(p => p.id_izvodjaca == izvodjac.id_korisnik);

                    _context.pripada.RemoveRange(pripada);
                }

                prijava.obradjeno = DateTime.Now;
                prijava.admin_komentar = dto.admin_komentar;

                _context.prijave.Update(prijava);

                _context.SaveChanges();
                transaction.Commit();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return BadRequest("Transaction failed");
            }
        }


















    }
}
