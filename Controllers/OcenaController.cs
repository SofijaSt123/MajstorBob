using AutoMapper;
using Majstor_bob.Dto;
using Majstor_bob.Enums;
using Majstor_bob.Helper;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Majstor_bob.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;

namespace Majstor_bob.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class OcenaController : Controller
    {
        private readonly IOcenaRepository _oceneRepositrory;
        private readonly IMapper _mapper;
        private readonly IKlijentiRepository _klijentiRepository;
        private readonly IKorisniciRepository _korisniciRepository;
        private readonly IMajstoriRepository _majstorRepository;
        private readonly IFirmeRepository _firmeRepository;
        public OcenaController(IOcenaRepository oceneRepositrory, IMapper mapper, IKlijentiRepository klijentiRepository,
            IKorisniciRepository korisniciRepository, IMajstoriRepository majstoriRepository, IFirmeRepository firmeRepository)
        {
            _mapper = mapper;
            _oceneRepositrory = oceneRepositrory;
            _klijentiRepository = klijentiRepository;
            _korisniciRepository = korisniciRepository;
            _majstorRepository = majstoriRepository;
            _firmeRepository = firmeRepository;
        }

        [Authorize]
        [HttpPost("Ostavirecenziju")]
        public IActionResult Recenzija([FromBody]OceneDto dto,int idIzvodjaca)
        {
            int klijentId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out klijentId))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsKlijent(User))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_klijentiRepository.KlijentExists(klijentId))
                return BadRequest("Klijent not found");

            if (!_korisniciRepository.KorisnikExists(idIzvodjaca))
                return BadRequest("Izvodjac ne postoji");

            var izvodjac = _korisniciRepository.GetKorisnikId(idIzvodjaca);
            if (izvodjac.tip_korisnika != Tip_korisnika.Majstor && izvodjac.tip_korisnika != Tip_korisnika.Firma)
                return BadRequest("Ovo nije izvodjac");

            if ((dto.ocena_cena > 5 || dto.ocena_cena < 1) ||
                (dto.ocena_brzine > 5 || dto.ocena_brzine < 1) ||
                (dto.ocena_kvaliteta > 5 || dto.ocena_kvaliteta < 1) ||
                (dto.ocena_odnosa > 5 || dto.ocena_odnosa < 1))
                return BadRequest("Invalid ocena");


            var ocene = new ocene
            {
                ocena_brzine = dto.ocena_brzine,
                ocena_cena = dto.ocena_cena,
                ocena_kvaliteta=dto.ocena_kvaliteta,
                ocena_odnosa = dto.ocena_odnosa,
                opis_recenzije=dto.opis_recenzije,

                id_klijenta=klijentId,
                id_izvodjaca=idIzvodjaca
            };
            if(!_oceneRepositrory.CreateOcenu(ocene))
                return BadRequest("Something went wrong");

            decimal novaOcena = ocene.ocena_cena + ocene.ocena_kvaliteta
                            + ocene.ocena_brzine + ocene.ocena_odnosa;
            novaOcena = novaOcena / 4;

            if (izvodjac.tip_korisnika == Tip_korisnika.Majstor)
            {
                var majstor = _majstorRepository.GetMajstorId(izvodjac.id_korisnik);

                majstor.broj_recenzija++;

                majstor.prosek_ocena =
                    (majstor.prosek_ocena * (majstor.broj_recenzija - 1) + novaOcena) /
                    majstor.broj_recenzija;

                _majstorRepository.UpdateMajstor(majstor);
            }

            if (izvodjac.tip_korisnika == Tip_korisnika.Firma)
            {
                var firma = _firmeRepository.GetFirmuId(izvodjac.id_korisnik);

                firma.broj_recenzija++;

                firma.prosek_ocena =
                    (firma.prosek_ocena * (firma.broj_recenzija - 1) + novaOcena) /
                    firma.broj_recenzija;

                _firmeRepository.UpdateFirmu(firma);
            }

            return Ok("Uspesno ostavljena recenzija"); //nznm sta da saljem nazad moza hoce da vidi ocenu
        }

        [HttpGet("Vratiocenemajstora")]
        public IActionResult GetOcene(int idIzvodjaca)
        {

            if (!_korisniciRepository.KorisnikExists(idIzvodjaca))
                return BadRequest("Izvodjac ne postoji");

            var izvodjac = _korisniciRepository.GetKorisnikId(idIzvodjaca);
            if (izvodjac.tip_korisnika != Tip_korisnika.Majstor && izvodjac.tip_korisnika != Tip_korisnika.Firma)
                return BadRequest("Ovo nije izvodjac");

            var ocene = _mapper.Map<List<OceneDto>>(_oceneRepositrory.getIzvodjacOcene(idIzvodjaca));
            
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            return Ok(ocene); //nznm sta da saljem nazad moza hoce da vidi ocenu
        }

        [Authorize]
        [HttpPost("Odgovorinaocenu")]
        public IActionResult Respond([FromBody] string odg,int ocenaId)
        {
            int izvodjacId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out izvodjacId))
            {
                return Unauthorized();
            }

            var ocena = _oceneRepositrory.GetOcenaId(ocenaId);
            if (ocena == null)
                return BadRequest("Ocena not found");

            if (!AuthorizationController.IsAdminOrOwner(User,(ocena.id_izvodjaca).ToString()))
                return StatusCode(403);


            if (!_korisniciRepository.KorisnikExists(izvodjacId))
                return BadRequest("Izvodjac ne postoji");


            ocena.odgovor = odg;
            
            if (!_oceneRepositrory.UpdateOcena(ocena))
                return BadRequest("Something went wrong");

            return Ok(ocena); //nznm sta da saljem nazad moza hoce da vidi ocenu
        }

        [Authorize]
        [HttpDelete("DeleteOcenu/{ocenaId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteOcenu(int ocenaId)      //ocene
        {

            int klijentId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out klijentId))
            {
                return Unauthorized();
            }

            var ocena = _oceneRepositrory.GetOcenaId(ocenaId);
            if (ocena == null)
                return BadRequest("Ocena not found");

            if (!AuthorizationController.IsAdminOrOwner(User,ocena.id_klijenta.ToString()))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_oceneRepositrory.DeleteOcenu(ocena))
            {
                ModelState.AddModelError("", "Something went wrong deleting");
                return BadRequest(ModelState);
            }

            return Ok("deleted");

        }













    }
}
