using AutoMapper;
using Majstor_bob.Dto;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Majstor_bob.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Majstor_bob.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class GradoviController : Controller
    {
        private readonly IGradoviRepository _gradRepository;
        private readonly IMapper _mapper;
        public GradoviController(IGradoviRepository gradRepository, IMapper mapper)
        {
            _gradRepository = gradRepository;
            _mapper = mapper;
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateGradovi([FromQuery] string naziv_grada)
        {
            if (naziv_grada == null)
                return BadRequest("Naziv is empty");


            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_gradRepository.GradExistsNaziv(naziv_grada) || !_gradRepository.PrvoSlovoVeliko(naziv_grada))
                return StatusCode(422, ModelState);

            var grad = new gradovi
            {
                naziv_grada = naziv_grada
            };

            if (!_gradRepository.CreateGrad(grad))
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }

            return Ok(grad); //ne treba da vraca korisnik ng ja da vidim sta radi
        }

        [HttpDelete("{gradId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteCountry(int gradId)
        {
            if (!_gradRepository.GradExists(gradId))
            {
                return NotFound();
            }

            var gradToDelete = _gradRepository.GetGradoviId(gradId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_gradRepository.DeleteGrad(gradToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting Grad");
            }

            return NoContent();

        }

        [HttpGet("VratiGradove")]
        public IActionResult GetGradove()
        {
            var gradovi = _gradRepository.GetGradovi();

            if (gradovi == null || !gradovi.Any())
                return NotFound("Nema gradova");

            return Ok(gradovi); ;

        }


    }
}

