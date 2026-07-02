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
    public class PripadaController : Controller
    {
        private readonly IPripadaRepository _pripadaRepository;
        private readonly IKorisniciRepository _korisniciRepository;
        private readonly IKategorijeRepository _kategorijeRepository;

        public PripadaController(IPripadaRepository pripadaRepository, IKorisniciRepository korisniciRepository,IKategorijeRepository kategorijeRepository)
        {
            _pripadaRepository = pripadaRepository;
            _korisniciRepository = korisniciRepository;
            _kategorijeRepository = kategorijeRepository;
        }

        [Authorize]
        [HttpPost("povezi izvodja i kategoriju")]
        public IActionResult Povezi(int id_kategorije)     //majstoru/firmi dodelis kategoriju
        {
            var claim = User.FindFirst(ClaimTypes.Sid).Value;
            if (claim == null)
                return Unauthorized("Missing user id in token");

            int korisnikId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);


            var korisnik = _korisniciRepository.GetKorisnikId(korisnikId);
            if (korisnik == null)
                return NotFound();

            if (!AuthorizationController.IsAdminOrOwner(User, korisnikId.ToString()))
                return Forbid();

            var kategorija = _kategorijeRepository.GetKategoriju(id_kategorije);

            if (kategorija == null)
                return NotFound();

            if (korisnik.tip_korisnika != Tip_korisnika.Majstor &&
        korisnik.tip_korisnika != Tip_korisnika.Firma)
            {
                return Forbid();
            }

            var veza = new pripada
            {
                id_izvodjaca = korisnikId,
                id_kategorije = id_kategorije
            };

            _pripadaRepository.CreatePripada(veza);

            return Ok("Povezano");

        }

        [Authorize]
        [HttpDelete("ObrisiKategorijuIzvodjaca")]
        public IActionResult brisi(int id_kategorije)     //majstoru/firmi brise kategoriju
        {
            var claim = User.FindFirst(ClaimTypes.Sid).Value;
            if (claim == null)
                return Unauthorized("Missing user id in token");

            int korisnikId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);


            var korisnik = _korisniciRepository.GetKorisnikId(korisnikId);
            if (korisnik == null)
                return NotFound();

            if (!AuthorizationController.IsAdminOrOwner(User, korisnikId.ToString()))
                return Forbid();

            var kategorija = _kategorijeRepository.GetKategoriju(id_kategorije);

            if (kategorija == null)
                return NotFound();

            if (korisnik.tip_korisnika != Tip_korisnika.Majstor &&
        korisnik.tip_korisnika != Tip_korisnika.Firma)
            {
                return Forbid();
            }

            var pripada = _pripadaRepository.GetPripada(korisnikId, id_kategorije);
            if (pripada == null)
                return NotFound();

            if (!_pripadaRepository.DeletePripada(pripada))
                return BadRequest("Problem saving chages in database");

            return Ok("izbrisano");

        }




    }
}
