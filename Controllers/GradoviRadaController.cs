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
using System.Security.Claims;

namespace Majstor_bob.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradoviRadaController :Controller
    {
        private readonly IGradoviradaRepository _gradoviradRepositrory;
        private readonly IKorisniciRepository _korisniciRepository;
        private readonly IMapper _mapper;
        private readonly IGradoviRepository _gradoviRepository;
        private readonly IKategorijeRepository _kategorijeRepository;
        private readonly IPripadaRepository _pripadaRepository;
        public GradoviRadaController(IGradoviradaRepository gradoviradRepositrory, IMapper mapper,IKorisniciRepository korisniciRepository,
            IGradoviRepository gradoviRepository,IKategorijeRepository kategorijeRepository
            ,IPripadaRepository pripadaRepository)
        {
            _gradoviradRepositrory = gradoviradRepositrory;
            _mapper = mapper;
            _korisniciRepository = korisniciRepository;
            _gradoviRepository = gradoviRepository;
            _kategorijeRepository = kategorijeRepository;
            _pripadaRepository= pripadaRepository;
        }
            
        [Authorize]
        [HttpPost("dodajGradGada")]
        public IActionResult DodajGradRada([FromBody] GradoviRadaDto grad)                  //Dodavanje grada gde rade
        {
            int id_izvodjaca;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_izvodjaca))
            {
                return Unauthorized();
            }

             var korisnik = _korisniciRepository.GetKorisnikId(id_izvodjaca);
            var gradic = _gradoviRepository.GetGradoviId(grad.id_grada);

            if (korisnik == null || gradic ==null ) return NotFound();

            if (korisnik.tip_korisnika != Tip_korisnika.Majstor &&
                korisnik.tip_korisnika != Tip_korisnika.Firma)
                BadRequest("Nema grad rada za tebe nisi bio dobar"); ;


            var gradRada = new gradovi_rada
                {
                    id_izvodjaca = id_izvodjaca,
                    id_grada = grad.id_grada,
                    zona_rada = grad.zona_rada,
                    doplata = grad.doplata,
                };

                _gradoviradRepositrory.CreateGradRada(gradRada);
                return Ok("Grad dodat");
            
        }


        //[Authorize]
        [HttpGet("GetIzvodjace")]
        public IActionResult GetIzvodjace([FromQuery] int id_kategorije, [FromQuery] int id_grad
        , [FromQuery] int? ocena,bool doplata)
        {

            if (ocena == null)
            {
                var izvodjaci = _gradoviradRepositrory.GetIzvodjacByGrad(id_grad,doplata);

                var filtrirani = _pripadaRepository.GetIzvodjaciZaKategoriju(id_kategorije, izvodjaci);

                var dto = filtrirani.Select(i => new VratiIzvodjaceDto
                {
                    Id = i.id_korisnik,
                    Ime = i.ime,
                    NazivFirme = i.firme?.naziv_firme,
                    ProsekOcena = i.tip_korisnika == Tip_korisnika.Firma
                    ? i.firme.prosek_ocena
                    : i.majstori.prosek_ocena,
                    BrojRecenzija = i.tip_korisnika == Tip_korisnika.Firma
                    ? i.firme.broj_recenzija
                    : i.majstori.broj_recenzija
                }).ToList();

                return Ok(dto);
            }
            if (ocena > 4 || ocena < 1)
                return BadRequest("Invalid ocena");

            var izvodjac = _gradoviradRepositrory.GetIzvodjacByGrad(id_grad, doplata);

            var filtriran = _pripadaRepository.GetIzvodjaciZaKategoriju(id_kategorije, izvodjac);

            var result = filtriran
                .Where(i =>
                (i.tip_korisnika == Tip_korisnika.Majstor && i.majstori.prosek_ocena >= ocena)
                ||
                (i.tip_korisnika == Tip_korisnika.Firma && i.firme.prosek_ocena >= ocena))
                .Select(i => new VratiIzvodjaceDto
                 {
                 Id = i.id_korisnik,
                Ime = i.ime,
                NazivFirme = i.firme?.naziv_firme,
                ProsekOcena = i.tip_korisnika == Tip_korisnika.Firma
                ? i.firme.prosek_ocena
                : i.majstori.prosek_ocena,
                BrojRecenzija = i.tip_korisnika == Tip_korisnika.Firma
                ? i.firme.broj_recenzija
                : i.majstori.broj_recenzija
                }).ToList();

            return Ok(result);



        }

        [Authorize]
        [HttpDelete("DeleteGradRada/{grad_id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteKorisnik(int grad_id)      //Brisanje korisnika 
        {
          

            int id_user;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_user))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsAdminOrOwner(User,id_user.ToString()))
                return StatusCode(403);

            var gradRad=_gradoviradRepositrory.GetGradRadaId(grad_id);

            if (gradRad==null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_gradoviradRepositrory.DeleteGradRada(gradRad))
            {
                ModelState.AddModelError("", "Something went wrong deleting Grad");
                return BadRequest(ModelState);
            }

            return Ok("deleted");

        }


        [HttpGet("GetGradRada")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult GetGradRada([FromQuery] int? id_izvodjaca)      //Brisanje korisnika 
        {

            int id_user;
            if (id_izvodjaca == null)
            {
                
                var claim = User.FindFirstValue(ClaimTypes.Sid);
                if (!int.TryParse(claim, out id_user))
                {
                    return Unauthorized();
                }
            }
            else
            {
                id_user = id_izvodjaca.Value;
            }
            var korisnik = _korisniciRepository.GetKorisnikId(id_user);

            if (korisnik == null || (korisnik.tip_korisnika!=Tip_korisnika.Majstor && korisnik.tip_korisnika!=Tip_korisnika.Firma))
                return Unauthorized();

            var gradRad = _gradoviradRepositrory.GetGradoveIzvodjaca(id_user);
            var dto = _mapper.Map<List<GetGradoviRadaDto>>(gradRad);
            
            return Ok(dto);

        }





    }
}
