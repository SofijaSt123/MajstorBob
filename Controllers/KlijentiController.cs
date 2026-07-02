using AutoMapper;
using Majstor_bob.Data;
using Majstor_bob.Dto;
using Majstor_bob.Helper;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Majstor_bob.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Xml;

namespace Majstor_bob.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KlijentiController : Controller
    {

        private readonly IKlijentiRepository _klijentiRepositrory;
        private readonly IKorisniciRepository _korisniciRepositrory;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly IKorisniciRepository _korisniciRepository;
        public KlijentiController(IKlijentiRepository klijentiRepositrory, IMapper mapper, IKorisniciRepository korisniciRepositrory
            , AppDbContext context,IKorisniciRepository korisniciRepository)
        {
            _klijentiRepositrory = klijentiRepositrory;
            _mapper = mapper;
            _korisniciRepositrory = korisniciRepositrory;
            _context = context;
            _korisniciRepository = korisniciRepository;
        }

        [Authorize]
        [HttpPut("UpdateGradRada")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateGradRada([FromBody] KlijentDto dto)   //ovo je samo update grad rada
        {
            if (!AuthorizationController.IsAdminOrOwner(User, dto.id_klijent.ToString()))
                return StatusCode(403);

            if (dto == null)
                return BadRequest(ModelState);

            if (!_klijentiRepositrory.KlijentExists(dto.id_klijent))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest();

            var klijent = _klijentiRepositrory.GetKlijenti(dto.id_klijent);
            klijent.id_grad_rada = dto.grad_rada;

            if (!_klijentiRepositrory.UpdateKlijent(klijent))
            {
                ModelState.AddModelError("", "Something went wrong updating category");
                return StatusCode(500, ModelState);
            }

            return Ok("Uspeh");
        }

        [Authorize]
        [HttpGet("KlijentInfo")]
        public IActionResult GetKlijentInfo()
        {
            int id_klijent;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_klijent))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsKlijent(User))
                return StatusCode(403);

            var klijent = _korisniciRepositrory.GetKorisnikId(id_klijent);

            if (klijent==null)
                return BadRequest("Klijent doesnt exist");

            var klijentSend = new GetKlijentDto
            {
                email = klijent.email,
                ime = klijent.ime,
                prezime = klijent.prezime,
                telefon = klijent.telefon,
                tip_korisnika = klijent.tip_korisnika,
                datum_registacije=klijent.datum_registacije,
            };

            return Ok(klijentSend);

        }

        [Authorize]
        [HttpPut("UpdateKlijent")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateKlijent([FromBody] UpdateKlijentDto dto)   //Firma da doda naziv radno vreme broj radnika
        {
            int klijentId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out klijentId))
            {
                return Unauthorized();
            }

            

            if (!AuthorizationController.IsAdminOrOwner(User, klijentId.ToString()))
                return StatusCode(403);
            if (dto == null)
                return BadRequest(ModelState);

            var fKorisnik = _korisniciRepository.GetKorisnikId(klijentId);
            if (fKorisnik == null)
                return NotFound();

            var klijent = _klijentiRepositrory.GetKlijenti(klijentId);
            if (klijent == null)
            {
                ModelState.AddModelError("", "Something went wrong updating category");
                return StatusCode(500, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest();

            var trasnsaction = _context.Database.BeginTransaction();
            try
            {

                fKorisnik.ime = dto.ime;
                fKorisnik.prezime = dto.prezime;
                fKorisnik.telefon = dto.telefon;

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

        [Authorize]
        [HttpGet("GetKlijent")]
        public IActionResult GetKlijentInfo([FromQuery]int id_klijent)
        {

            var klijent = _korisniciRepositrory.GetKorisnikId(id_klijent);

            if (klijent == null)
                return BadRequest("Klijent doesnt exist");

            var klijentSend = new GetKlijentInfoDto
            {
                ime = klijent.ime,
                prezime = klijent.prezime,
                telefon = klijent.telefon,
                tip_korisnika = klijent.tip_korisnika,
            };

            return Ok(klijentSend);

        }


    }
}
