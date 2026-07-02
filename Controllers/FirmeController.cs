
using AutoMapper;
using Majstor_bob.Data;
using Majstor_bob.Dto;
using Majstor_bob.Helper;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Majstor_bob.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Majstor_bob.Helper.EncryptionHelp;

namespace Majstor_bob.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class FirmeController : Controller
    {
        private readonly IFirmeRepository _firmaRepository;
        private readonly IMapper _mapper;
        private readonly IKorisniciRepository _korisniciRepository;
        private readonly AppDbContext _context;
        public FirmeController(IFirmeRepository firmaRepository, IMapper mapper, IKorisniciRepository korisniciRepository,
            AppDbContext appDbContext)
        {
            _firmaRepository = firmaRepository;
            _mapper = mapper;
            _korisniciRepository = korisniciRepository;
            _context = appDbContext;
        }

        [Authorize]
        [HttpPut("UpdateFirma")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateFirma([FromBody] UpdateFirmeDto dto)   //Firma da doda naziv radno vreme broj radnika
        {
            int firmaId = 0;
            var claim=User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out firmaId))
            { 
                return Unauthorized();
            }

            if (dto.pocetak_radnog_vremena > dto.kraj_radnog_vremena ||
               dto.broj_ukupnih_radnika < dto.broj_dostupnih_radnika)
                return BadRequest("Ne dozvoljen konfiguracija podataka");



            if (!AuthorizationController.IsAdminOrOwner(User, firmaId.ToString()))
                return StatusCode(403);

            if (dto == null)
                return BadRequest(ModelState);

            var fKorisnik = _korisniciRepository.GetKorisnikId(firmaId);

            if (fKorisnik==null)
                return NotFound();

            var firma = _firmaRepository.GetFirmuId(firmaId);
            if (firma == null)
            {
                ModelState.AddModelError("", "Something went wrong updating category");
                return StatusCode(500, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest();

            var trasnsaction=_context.Database.BeginTransaction();
            try
            {
                firma.opis_usluga = dto.opis_usluga;
                firma.pocetak_radnog_vremena = dto.pocetak_radnog_vremena;
                firma.kraj_radnog_vremena = dto.kraj_radnog_vremena;
                firma.naziv_firme = dto.naziv_firme;
                firma.broj_ukupnih_radnika = dto.broj_ukupnih_radnika;
                firma.broj_dostupnih_radnika = dto.broj_dostupnih_radnika;

                fKorisnik.ime = dto.ime;
                fKorisnik.prezime = dto.prezime;
                fKorisnik.telefon = dto.telefon;

                if (!_firmaRepository.UpdateFirmu(firma))
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
            }catch(Exception ex)
            { 
                trasnsaction.Rollback();
             return StatusCode(500, ex); 
            }
        }

        [Authorize]
        [HttpGet("FirmaInfo")]
        public IActionResult GetFirmaInfo()
        {
            int id_firma;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_firma))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsFirma(User))
                return StatusCode(403);

            var firma = _korisniciRepository.GetKorisnikId(id_firma);
            var f = _firmaRepository.GetFirmuId(id_firma);

            if (firma == null)
                return BadRequest("Firma doesnt exist");

            if (f == null)
                return BadRequest("Firma doesnt exist");

            var firmaSend = new GetFirmaDto
            {
                stripe_num = firma.stripe_num,
                verifikovan =firma.verifikovan,
                email = firma.email,
                ime = firma.ime,
                prezime = firma.prezime,
                telefon = firma.telefon,
                tip_korisnika = firma.tip_korisnika,
                datum_registacije = firma.datum_registacije,
                kraj_radnog_vremena = f.kraj_radnog_vremena,
                opis_usluga = f.opis_usluga,
                pocetak_radnog_vremena = f.pocetak_radnog_vremena,
                broj_ukupnih_radnika=f.broj_ukupnih_radnika,
                naziv_firme=f.naziv_firme,
                broj_dostupnih_radnika=f.broj_dostupnih_radnika,
                profilna_slika = firma.profilna_slika != null
                ? Convert.ToBase64String(firma.profilna_slika)
                : null

            };

            return Ok(firmaSend);
        }

        [HttpGet("FirmaInfoForKlijent/{id_firma}")]
        public IActionResult GetFirmaInfoKlijent(int id_firma)
        {
        
            var firma = _korisniciRepository.GetKorisnikId(id_firma);
            var f = _firmaRepository.GetFirmuId(id_firma);

            if (firma == null)
                return BadRequest("Firma doesnt exist");

            if (f == null)
                return BadRequest("Firma doesnt exist");

            var firmaSend = new GetFirmaKlijentDto
            {
                stripe_num = firma.stripe_num,
                verifikovan = firma.verifikovan,
                ime = firma.ime,
                prezime = firma.prezime,
                telefon = firma.telefon,
                tip_korisnika = firma.tip_korisnika,
                kraj_radnog_vremena = f.kraj_radnog_vremena,
                opis_usluga = f.opis_usluga,
                pocetak_radnog_vremena = f.pocetak_radnog_vremena,
                broj_ukupnih_radnika = f.broj_ukupnih_radnika,
                naziv_firme = f.naziv_firme,
                broj_dostupnih_radnika = f.broj_dostupnih_radnika,
                broj_recenzija=f.broj_recenzija,
                prosek_ocena=f.prosek_ocena,
                profilna_slika = firma.profilna_slika != null
                ? Convert.ToBase64String(firma.profilna_slika)
                : null


            };

            return Ok(firmaSend);
        }

        [Authorize]
        [HttpPut("VerifyFirma")]

        public IActionResult Verify([FromBody] string pib)
        {
            int izvodjacId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out izvodjacId))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.isPIB(pib))
            {
                return BadRequest("Not pib");
            }

            var izvodjac = _firmaRepository.GetFirmuId(izvodjacId);
            var korisnik = _korisniciRepository.GetKorisnikId(izvodjacId);

            if (izvodjac == null || korisnik == null)
                return BadRequest("User doesnt exist");

            if (_firmaRepository.ExistPib(pib))
                return Unauthorized();

            var hashjmbg = HashHelper.Sha256(pib);
            var transation = _context.Database.BeginTransaction();

            try
            {

                izvodjac.pib = hashjmbg;
                korisnik.verifikovan = true;

                if (!_firmaRepository.UpdateFirmu(izvodjac))
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
