using AutoMapper;
using Majstor_bob.Data;
using Majstor_bob.Dto;
using Majstor_bob.Enums;
using Majstor_bob.Helper;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Majstor_bob.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Majstor_bob.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObavestenjeController : Controller
    {
        private readonly IObavestenjaRepository _obavestenjeRepository;
        private readonly IMapper _mapper;
        private readonly IKorisniciRepository _korisniciRepository;
        private readonly AppDbContext _context;
        
        public ObavestenjeController(IObavestenjaRepository obavestenjeRepository, IMapper mapper
            , IKorisniciRepository korisniciRepository, AppDbContext context)

        {
            _mapper = mapper;
            _obavestenjeRepository = obavestenjeRepository;
            _korisniciRepository = korisniciRepository;
        }

        
        
        [Authorize]
        [HttpPost]
        public IActionResult SendObavestenja([FromBody] CreateObavestenjaDto obv)
        {
            int id_admin;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_admin))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsAdmin(User))
                return StatusCode(403);

            if(!ModelState.IsValid)
                return BadRequest();

            if ((obv.receiver_id != null && obv.tip_kome_saljes != null) ||
                (obv.receiver_id == null && obv.tip_kome_saljes == null))
                return BadRequest("Invalid Requset");


            if (obv.receiver_id != null)
            {

                var korisnik=_korisniciRepository.GetKorisnikId(obv.receiver_id.Value);

                if (korisnik == null)
                    return BadRequest("Korisnik ne postoji.");

                var note = new obavestenja
                {
                    naslov = obv.naslov,
                    admin_id = id_admin,
                    receiver_id = obv.receiver_id,
                    procitanno = false,
                    tip_kome_saljes = null,
                    tip = ""
                };
                if (!_obavestenjeRepository.CreateObavestenje(note))
                    return BadRequest("Something went wrong createing obavestenje");

                return Ok("Uspesno poslato obavestenje");
            }



            ICollection<korisnici> korisnici;

            if (obv.tip_kome_saljes == Tip_obavestenja.Svi)
                korisnici = _korisniciRepository.GetKorisnicis();
            else if (obv.tip_kome_saljes == Tip_obavestenja.Klijenti)
                korisnici = _korisniciRepository.GetByTip(Tip_korisnika.Klijent);
            else if (obv.tip_kome_saljes == Tip_obavestenja.Izvodjaci)
                korisnici = _korisniciRepository.GetIzvodjaci();
            else
                return BadRequest("Invalid target group");

            if (korisnici == null || !korisnici.Any())
                return BadRequest("No users found");


            var list = korisnici.Select(k => new obavestenja
            {
                naslov = obv.naslov,
                admin_id = id_admin,
                receiver_id = k.id_korisnik,
                procitanno = false,
                tip = ""
            }).ToList();

            if (!_obavestenjeRepository.CreateObavestenja(list))
                return BadRequest("Error creating notifacations");

            return Ok("Broadcast sent");
        }
        //primalac_id=null \\tip=nesto --pravis redove sa tim tipovima
        //primalac_id=nesto //tip=null --pravis red satim korisnikom
        //primalac_id=nsto //tip=nesto --invlid Request


        [Authorize]
        [HttpGet("GetObavestenja")]
        public IActionResult GetObavestenje()
        {
            int id_korisnik;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out id_korisnik))
            {
                return Unauthorized();
            }

            var korisnik = _korisniciRepository.GetKorisnikId(id_korisnik);

            if (korisnik == null)
                return BadRequest("User doesnt exist");

            var obavestenja = _obavestenjeRepository.GetObavestenjaKorisnika(id_korisnik);

            if (obavestenja == null || !obavestenja.Any())
                return Ok();

            foreach (var o in obavestenja)
            {
                o.procitanno = true;
            }
            if (!_obavestenjeRepository.UpdateObavestenja(obavestenja))
                return BadRequest("Problem saving changes");

            var dto=_mapper.Map<List<GetObavestenjaDto>>(obavestenja);

            return Ok(dto);
        }






    }
}
