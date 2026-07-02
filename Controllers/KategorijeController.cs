using AutoMapper;
using Majstor_bob.Dto;
using Majstor_bob.Enums;
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
    public class KategorijeController : Controller
    {
        private readonly IKategorijeRepository _kategorijeRepositrory;
        private readonly IMapper _mapper;
        private readonly IKorisniciRepository _korisniciRepositrory;

        public KategorijeController(IKategorijeRepository kategorijeReposetory,IMapper mapper, IKorisniciRepository korisniciRepositrory)
        {
            _kategorijeRepositrory = kategorijeReposetory;
            _mapper = mapper;
            _korisniciRepositrory = korisniciRepositrory;
        }

        [HttpGet("Vrati sve kategorije")]
        [ProducesResponseType(200, Type = typeof(ICollection<kategorije>))]
        public IActionResult GetKategorije()
        {
            var kategorije = _mapper.Map<List<KategorijeDto>>(_kategorijeRepositrory.GetKategorije()); //_mapper.Map<tipdto>(obican poziv ne dto)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(kategorije);
        }


        [HttpGet("{kategorija_id}")]
        [ProducesResponseType(200, Type = typeof(kategorije))]
        public IActionResult GetKorisnik(int kategorija_id)
        {
            if (!_kategorijeRepositrory.CatagoryExists(kategorija_id))
                return NotFound();

            var kategorija = _mapper.Map<KategorijeDto>(_kategorijeRepositrory.GetKategoriju(kategorija_id));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(kategorija);
        }

        [HttpPost("Napravi kategoriju")]
        public IActionResult CreateKategorija([FromQuery] int idRod ,[FromBody] KategorijeDto dto)
        {
            if (dto == null)
                return BadRequest("Dto is empty");

            if (!ModelState.IsValid) 
           return BadRequest(ModelState);

            var kategorije = _mapper.Map<kategorije>(dto);
            if (idRod != 0 && _kategorijeRepositrory.CatagoryExists(idRod)) 
            {
                kategorije.id_roditelja = idRod;
            }

            _kategorijeRepositrory.CreateKategoriju(kategorije);

            return Ok(kategorije);

        }

        [Authorize]
        [HttpGet("GetIzvodjacKategorije")]
        public IActionResult GetKategorijeIzvodjaca()
        {
            int izvodjacId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);

            if (!int.TryParse(claim, out izvodjacId))
            {
                return Unauthorized();
            }

            var izvodjac = _korisniciRepositrory.GetKorisnikId(izvodjacId);

            if (izvodjac == null)
                return BadRequest("Nema korisnika");

            if (izvodjac.tip_korisnika != Tip_korisnika.Majstor &&
                izvodjac.tip_korisnika != Tip_korisnika.Firma)
                return Unauthorized();
            var kategorije = _kategorijeRepositrory.GetIzvodjacKategorije(izvodjacId);

            if (kategorije == null || !kategorije.Any())
                return NotFound("Izvodjac nema nijednu kategoriju");

            var kat = _mapper.Map< List < KategorijeDto >> (kategorije);

            return Ok(kat);
        }


    }
}
