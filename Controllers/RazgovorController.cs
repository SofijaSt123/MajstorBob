using AutoMapper;
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
    public class RazgovorController : Controller
    {
        private readonly IRazgovorRepository _razgovorRepositrory;
        private readonly IMapper _mapper;
        private readonly IKorisniciRepository _korisniciRepository;
        public RazgovorController(IRazgovorRepository razgovorRepositrory, IMapper mapper, IKorisniciRepository korisniciRepository)
        {
            _razgovorRepositrory = razgovorRepositrory;
            _mapper = mapper;
            _korisniciRepository = korisniciRepository;
        }

        [Authorize]
        [HttpPost("CreateRazgovor/{id_recipient}")]
        public IActionResult Post(int id_recipient)
        {
            int id_sender;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_sender))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsKlijent(User))
                return StatusCode(403);

            var izvodjac = _korisniciRepository.GetKorisnikId(id_recipient);

            if(izvodjac==null)
                return NotFound();

            if (izvodjac.tip_korisnika != Tip_korisnika.Majstor && izvodjac.tip_korisnika != Tip_korisnika.Firma)
                return Unauthorized();

            if (_razgovorRepositrory.RazgovorExists(id_recipient, id_sender))
            {
                var existingRazgovor = _razgovorRepositrory.GetRazgovorByIds(id_recipient, id_sender);
                return Ok(existingRazgovor);
            }

            var razgovor = new razgovor
            {
                id_klijent = id_sender,
                id_majstor = id_recipient
            };

            if(!_razgovorRepositrory.CreateRazgovor(razgovor))
                return BadRequest("Problem while creating chat");

            return Ok();
        }


        [Authorize]
        [HttpGet("GetRazgovore")]
        public IActionResult Pull()
        {
            int id_user;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_user))
            {
                return Unauthorized();
            }


            var izvodjac = _korisniciRepository.GetKorisnikId(id_user);

            if (izvodjac == null)
                return NotFound();

            var razgovor = _razgovorRepositrory.GetRazgovoreByUserId(id_user,izvodjac.tip_korisnika);

            if(razgovor==null || !razgovor.Any())
                return Ok("Nema razgovora");

            return Ok(razgovor);
        }


    }
}
