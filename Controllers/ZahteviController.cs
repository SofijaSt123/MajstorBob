using AutoMapper;
using Humanizer;
using Majstor_bob.Data;
using Majstor_bob.Dto;
using Majstor_bob.Enums;
using Majstor_bob.Helper;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Majstor_bob.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Majstor_bob.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZahteviController : Controller
    {
        private readonly IZahteviRepository _zahteviRepository;
        private readonly TokenProvider _tokenProvider;
        private readonly IKorisniciRepository _korisniciRepository;
        private readonly IKlijentiRepository _klijentiRepository;
        private readonly IZakazivanjeRepository _zakazivanjeRepository;

        public ZahteviController(IZahteviRepository zahteviRepository, TokenProvider tokenProvider
            , IKlijentiRepository klijentiRepository, IKorisniciRepository korisniciRepository
            , IZakazivanjeRepository zakazivanjeRepository)
        {
            _tokenProvider = tokenProvider;
            _zahteviRepository = zahteviRepository;
            _klijentiRepository = klijentiRepository;
            _korisniciRepository = korisniciRepository;
            _zakazivanjeRepository = zakazivanjeRepository;
        }

        [Authorize] 
        [HttpPost("SendRequest")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateZahtev([FromBody] ZahtevDto dto) //Registracija za korisnike
        {

            int klijentId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out klijentId))
            {
                return Unauthorized();
            }

            if (klijentId == 0)
                return BadRequest("Invalid token");
            if (!AuthorizationController.IsKlijent(User))
                return StatusCode(403);


            if (dto == null)
                return BadRequest("Dto is empty");



            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_klijentiRepository.KlijentExists(klijentId))
                return BadRequest("Klijent not found");


            var izvodjac = _korisniciRepository.GetKorisnikId(dto.id_izvodjaca);
            if (izvodjac == null)
                BadRequest("Korisnik vise ne postoji");

            if (izvodjac.tip_korisnika != Tip_korisnika.Majstor && izvodjac.tip_korisnika != Tip_korisnika.Firma)
                return BadRequest("Ovo nije izvodjac");

            var zahtev = new zahtevi
            {
                id_klijenta = klijentId,
                id_izvodjaca = dto.id_izvodjaca,
                datum = dto.datum,
                opis_radova = dto.opis_radova,
                adresa = dto.adresa,
                vreme = dto.vreme,
                status = Stanje_zahteva.Cekanje
            };

            if (!_zahteviRepository.CreateZahtev(zahtev))
            {
                ModelState.AddModelError("", "Greska pri pravljenju zahteva");
                return StatusCode(500, ModelState);
            }

            return Ok("Zahtev je poslat");
        }


        [Authorize]
        [HttpGet("GetZahtevi")]
        public IActionResult GetZahtevi(Stanje_zahteva? stanje)
        {
            int izvodjacId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out izvodjacId))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsAdminOrOwner(User, izvodjacId.ToString()))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var izvodjac = _korisniciRepository.GetKorisnikId(izvodjacId);
            if (izvodjac == null)
                return BadRequest("Korisnik vise ne postoji");

            if (Tip_korisnika.Klijent==izvodjac.tip_korisnika)
            {

                ICollection<ListaZahtevaDto> dtok = _zahteviRepository.GetZahteveKlijenta(izvodjacId);
                if (dtok == null)
                    return Ok("Nema zahteva");

                return Ok(dtok);
            }

            if (!stanje.HasValue)
            {
                ICollection<ListaZahtevaDto> lista = _zahteviRepository.GetZahteveIzvodjaca(izvodjacId);
                if (lista == null)
                    return Ok("Nema zahteva");
                return Ok(lista);
            }
            if (stanje != Stanje_zahteva.Cekanje && stanje != Stanje_zahteva.Odbijeno &&
                   stanje != Stanje_zahteva.Prihvaceno)
                return BadRequest("Invalidno stanje");

            ICollection<ListaZahtevaDto> dto = _zahteviRepository.GetZahteveIzvodjacaByStanje(izvodjacId, stanje.Value);
            if (dto == null)
                return Ok("Nema zahteva");

            return Ok(dto);
        }

      

        [Authorize]
        [HttpPost("ZahtevAnswear")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]

        public IActionResult Respond([FromQuery] bool? prihvati, [FromBody] SendZakazivanjeDto dto)
        {
            int izvodjacId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out izvodjacId))
            {
                return Unauthorized();
            }

            if (prihvati == null)
                return BadRequest("Mising field");

            if (!AuthorizationController.IsAdminOrOwner(User, izvodjacId.ToString()))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto == null)
                return BadRequest(ModelState);

            if (prihvati == false)
            {
                var zahtev = _zahteviRepository.GetZahtevById(dto.id_zahteva);
                if (zahtev == null)
                    return BadRequest(ModelState);
                zahtev.status = Stanje_zahteva.Odbijeno;

                if (!_zahteviRepository.UpdateZahtev(zahtev))
                    return BadRequest("Invalid");

                return Ok("Zahtev je odbijen");
            }

            var zakaz = new zakazivanje
            {
                id_zahteva = dto.id_zahteva,
                opis = dto.opis,
                cena_donja = dto.cena_donja,
                cena_gornja = dto.cena_gornja,
                pocetak = dto.pocetak,
                datum = dto.datum,
                da_li_je_placeno = false,
                kraj = dto.kraj,
                status = Status_zakazivanja.Poslato
            };

            if (dto == null)
                return BadRequest(ModelState);

            if (zakaz.cena_donja > zakaz.cena_gornja)
                return BadRequest("Neispravan opseg cena");

            if (zakaz.pocetak >= zakaz.kraj)
                return BadRequest("Neispravno vreme");

            if (!_zakazivanjeRepository.CreateZakazivanje(zakaz))

                return BadRequest("Invalid Zakazivanje");

            var zah = _zahteviRepository.GetZahtevById(dto.id_zahteva);
            if (zah == null)
                return BadRequest(ModelState);
            zah.status = Stanje_zahteva.Prihvaceno;


            if (!_zahteviRepository.UpdateZahtev(zah))
                return BadRequest("Something went wrong updating");

            return Ok("Zahtev je prihvacen");

        }

        [Authorize]
        [HttpPost("KlijentOdgovorNaZakazivanje")]
        public IActionResult ZakazivanjeResponse([FromQuery] bool? response, [FromQuery] int idZakazivanja)
        {
            int klijentId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out klijentId))
            {
                return Unauthorized();
            }

            if (response == null)
                return BadRequest("Mising field");

            if (!AuthorizationController.IsKlijent(User))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var zakazivanje = _zakazivanjeRepository.GetZakazivanjeId(idZakazivanja);

            if (zakazivanje == null)
                return BadRequest("Zakazivanje doesnt exist");



            if (klijentId != zakazivanje.zahtev.id_klijenta)
                return BadRequest("Nemas prava da odg na ovo");

            if (response == true)
            {
                zakazivanje.status = Status_zakazivanja.Prihvaceno;
                if (!_zakazivanjeRepository.UpdateZakazivanje(zakazivanje))
                    return BadRequest("Something went wrong while saving");
                return Ok("Zakazivanje je uspesno prihvacen");

            }
            zakazivanje.status = Status_zakazivanja.Odbijeno;

            if (!_zakazivanjeRepository.UpdateZakazivanje(zakazivanje))
                return BadRequest("Something went wrong while saving");

            return Ok("Zakazivanje je uspesno odbijeno");

        }

        [Authorize]
        [HttpGet("GetZakazivanje")]
        public IActionResult GetZakazivanje()
        {
            int klijentId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out klijentId))
            {
                return Unauthorized();
            }


            var korisnik=_korisniciRepository.GetKorisnikId(klijentId);
            if(korisnik==null)
                return BadRequest("User doesnt exist");

            if (korisnik.tip_korisnika == Tip_korisnika.Klijent)
            {

                var zakazivanja = _zakazivanjeRepository.GetZakazivanjaKlijenta(klijentId);

                if (zakazivanja == null || !zakazivanja.Any())
                    return Ok("Nema zakazivanja");

                return Ok(zakazivanja);
            }
            var zakaz = _zakazivanjeRepository.GetZakazivanjaIzvodjaca(klijentId);

            if (zakaz == null || !zakaz.Any())
                return Ok("Nema zakazivanja");

            return Ok(zakaz);

        }









    }
}
