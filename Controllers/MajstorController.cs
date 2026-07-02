using AutoMapper;
using Majstor_bob.Data;
using Majstor_bob.Dto;
using Majstor_bob.Helper;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Majstor_bob.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using static Majstor_bob.Helper.EncryptionHelp;

namespace Majstor_bob.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MajstorController : Controller
    {
        private readonly IMajstoriRepository _majstoriRepository;
        private readonly IMapper _mapper;
        private readonly IKorisniciRepository _korisniciRepository;
        private readonly AppDbContext _context;
        public MajstorController(IMajstoriRepository majstoriRepository, IMapper mapper, IKorisniciRepository korisniciRepository
           , AppDbContext context)
        {
            _majstoriRepository = majstoriRepository;
            _mapper = mapper;
            _korisniciRepository = korisniciRepository;
            _context = context;
        }





        [Authorize]
        [HttpGet("MajstorInfo")]
        public IActionResult GetMajstorInfo()
        {
            int id_majstor;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_majstor))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsMajstor(User))
                return StatusCode(403);

            var majstor = _korisniciRepository.GetKorisnikId(id_majstor);
            var m = _majstoriRepository.GetMajstorId(id_majstor);

            if (majstor == null)
                return BadRequest("Majstor doesnt exist");

            if (majstor == null)
                return BadRequest("Majstor doesnt exist");

            var majstorSend = new GetMajstorDto
            {
                stripe_num = majstor.stripe_num,
                verifikovan=majstor.verifikovan,
                email = majstor.email,
                ime = majstor.ime,
                prezime = majstor.prezime,
                telefon = majstor.telefon,
                tip_korisnika = majstor.tip_korisnika,
                datum_registacije = majstor.datum_registacije,
                kraj_radnog_vremena = m.kraj_radnog_vremena,
                opis_usluga = m.opis_usluga,
                pocetak_radnog_vremena = m.pocetak_radnog_vremena,
                profilna_slika = majstor.profilna_slika != null
                ? Convert.ToBase64String(majstor.profilna_slika)
                : null

            };

            return Ok(majstorSend);

        }

        [Authorize]
        [HttpPut("UpdateMajstor")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateMajstor([FromBody] UpdateMajstorDto dto)   //Firma da doda naziv radno vreme broj radnika
        {
            int majstorId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out majstorId))
            {
                return Unauthorized();
            }

            if (dto.pocetak_radnog_vremena > dto.kraj_radnog_vremena)
                return BadRequest("Ne dozvoljen konfiguracija podataka");


            if (!AuthorizationController.IsAdminOrOwner(User, majstorId.ToString()))
                return StatusCode(403);

            if (dto == null)
                return BadRequest(ModelState);

            var fKorisnik = _korisniciRepository.GetKorisnikId(majstorId);

            if (fKorisnik == null)
                return NotFound();

            var majstor = _majstoriRepository.GetMajstorId(majstorId);
            if (majstor == null)
            {
                ModelState.AddModelError("", "Something went wrong updating category");
                return StatusCode(500, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest();

            var trasnsaction = _context.Database.BeginTransaction();
            try
            {
                majstor.opis_usluga = dto.opis_usluga;
                majstor.pocetak_radnog_vremena = dto.pocetak_radnog_vremena;
                majstor.kraj_radnog_vremena = dto.kraj_radnog_vremena;

                fKorisnik.ime = dto.ime;
                fKorisnik.prezime = dto.prezime;
                fKorisnik.telefon = dto.telefon;

                if (!_majstoriRepository.UpdateMajstor(majstor))
                {
                    trasnsaction.Rollback();
                    ModelState.AddModelError("", "Something went wrong updating category");
                    return StatusCode(500, ModelState);
                }
                if (!_korisniciRepository.UpdateKorisnik(fKorisnik))
                {
                    trasnsaction.Rollback();
                    ModelState.AddModelError("", "Something went wrong updating category");
                    return StatusCode(500, ModelState);
                }


                trasnsaction.Commit();
                return Ok("Uspeh");
            }
            catch (Exception ex)
            {
                trasnsaction.Rollback();
                return StatusCode(500, ex);
            }
        }


        [HttpGet("MajstorInfoForKlijent/{id_majstor}")]
        public IActionResult GetMajstorInfoKlijent(int id_majstor)
        {

            var majstor = _korisniciRepository.GetKorisnikId(id_majstor);
            var m = _majstoriRepository.GetMajstorId(id_majstor);

            if (majstor == null)
                return BadRequest("Majstor doesnt exist");

            if (majstor == null)
                return BadRequest("Majstor doesnt exist");

            var majstorSend = new GetMajstorKijentDto
            {
                stripe_num = majstor.stripe_num,
                verifikovan=majstor.verifikovan,
                ime = majstor.ime,
                prezime = majstor.prezime,
                telefon = majstor.telefon,
                tip_korisnika = majstor.tip_korisnika,
                kraj_radnog_vremena = m.kraj_radnog_vremena,
                opis_usluga = m.opis_usluga,
                pocetak_radnog_vremena = m.pocetak_radnog_vremena,
                prosek_ocena = m.prosek_ocena,
                broj_recenzija = m.broj_recenzija,
                profilna_slika= majstor.profilna_slika != null
                ? Convert.ToBase64String(majstor.profilna_slika)
                : null
            };

            return Ok(majstorSend);

        }

        [Authorize]
        [HttpPut("VerifyMajstor")]

        public IActionResult Verify([FromBody] string jmbg)
        {
            int majstorId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out majstorId))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsJmbg(jmbg))
            {
                return BadRequest("Not jmbg");
            }

            var izvodjac = _majstoriRepository.GetMajstorId(majstorId);
            var korisnik = _korisniciRepository.GetKorisnikId(majstorId);

            if (izvodjac == null || korisnik == null)
                return BadRequest("User doesnt exist");

            if (_majstoriRepository.ExistJmbg(jmbg))
                return Unauthorized();

            var hashjmbg = HashHelper.Sha256(jmbg);
            var transation = _context.Database.BeginTransaction();

            try
            {

                izvodjac.jmbg = hashjmbg;
                korisnik.verifikovan = true;

                if (!_majstoriRepository.UpdateMajstor(izvodjac))
                {
                    transation.Rollback();
                    return BadRequest("Something went wrong saving");
                }

                if (!_korisniciRepository.UpdateKorisnik(korisnik))
                {
                    transation.Rollback();
                    return BadRequest("Something went wrong saving");
                }

                transation.Commit();
                return Ok("Uspesna verifikacija");

            }
            catch (Exception ex)
            {
                transation.Rollback();
                return BadRequest(ex.ToString());
            }

        }






    }
}
