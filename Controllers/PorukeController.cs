using AutoMapper;
using Majstor_bob.Dto;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace Majstor_bob.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class PorukeController : Controller
    {
        private readonly IPorukeRepository _porukeRepositrory;
        private readonly IRazgovorRepository _rargovorRepositrory;
        private readonly IMapper _mapper;
        public PorukeController(IPorukeRepository porukeRepositrory, IMapper mapper, IRazgovorRepository rargovorRepositrory)
        {
            _porukeRepositrory = porukeRepositrory;
            _mapper = mapper;
            _rargovorRepositrory = rargovorRepositrory;
        }

        [Authorize]
        [HttpPost("Send/{id_razgovora}")]
        public IActionResult sendPoruke([FromBody] string tekst,int id_razgovora)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int id_sender;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_sender))
            {
                return Unauthorized();
            }

            var razgovor = _rargovorRepositrory.GetRazgovorId(id_razgovora);
            if (razgovor == null)
                return BadRequest("Razgovor ne postoji");

            if (razgovor.id_klijent != id_sender && razgovor.id_majstor != id_sender)
                return Forbid();

            if (string.IsNullOrWhiteSpace(tekst))
                return BadRequest("Poruka ne sme biti prazna");

            var poruka = new poruke
            {
                id_razgovora = id_razgovora,
                posiljac_id = id_sender,
                tekst = tekst,
                vreme = DateTime.Now
            };

            if (!_porukeRepositrory.CreatePoruke(poruka))
                return BadRequest("Something went wrong while saving");

            return Ok("Sent");

        }

        [Authorize]
        [HttpGet("GetPoruke/{id_razgovora}")]
        public IActionResult GetPoruke(int id_razgovora)
        {
            int id_sender;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_sender))
            {
                return Unauthorized();
            }

            var razgovor = _rargovorRepositrory.GetRazgovorId(id_razgovora);
            if (razgovor == null)
                return BadRequest("Razgovor ne postoji");

            if (razgovor.id_klijent != id_sender && razgovor.id_majstor != id_sender)
                return Forbid();

            var poruke = _porukeRepositrory.GetPoruke(id_razgovora);

            var pok = _mapper.Map<List<GetPorukeDto>>(poruke);

            return Ok(pok);
        }














    }
}
