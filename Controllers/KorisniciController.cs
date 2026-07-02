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
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Formats;
using Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Majstor_bob.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KorisniciController : Controller
    {
        private readonly IKorisniciRepository _korisniciRepositrory;
        private readonly IMajstoriRepository _majstoriRepository;
        private readonly IFirmeRepository _firmeRepository;
        private readonly IKlijentiRepository _klijentiRepository;
        private readonly IMapper _mapper;
        private readonly IKategorijeRepository _kategorijeRepository;
        private readonly IPripadaRepository _pripadaRepository;
        private readonly TokenProvider _tokenProvider;
        private readonly AppDbContext _context;
        private readonly IGradoviradaRepository _gradoviradaRepository;
        private readonly StripeSettings _stripeSettings;
        public KorisniciController(IKorisniciRepository korisniciRepositrory, IMapper mapper, TokenProvider tokenProvider
            , IMajstoriRepository majstoriRepository, IFirmeRepository firmeRepository, AppDbContext context
            , IKlijentiRepository klijentiRepository, IKategorijeRepository kategorijeRepository, IPripadaRepository pripadaRepository
            , IGradoviradaRepository gradoviradaRepository, IOptions<StripeSettings> stripeSettings)
        {
            _korisniciRepositrory = korisniciRepositrory;
            _mapper = mapper;
            _tokenProvider = tokenProvider;
            _majstoriRepository = majstoriRepository;
            _firmeRepository = firmeRepository;
            _context = context;
            _klijentiRepository = klijentiRepository;
            _pripadaRepository = pripadaRepository;
            _kategorijeRepository = kategorijeRepository;
            _gradoviradaRepository = gradoviradaRepository;
            _stripeSettings = stripeSettings.Value;

        }
        /*
        [HttpGet("GetKorinike")]
        [ProducesResponseType(200, Type = typeof(ICollection<korisnici>))]
        public IActionResult GetKorisnike()
        {
            var korisnici = _mapper.Map<List<KorisniciDto>>(_korisniciRepositrory.GetKorisnicis()); //_mapper.Map<tipdto>(obican poziv ne dto)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(korisnici);
        }
        */

        [Authorize]
        [HttpGet("id/{id}")]
        [ProducesResponseType(200, Type = typeof(korisnici))]
        public IActionResult GetKorisnik(int id)
        {

            if (!AuthorizationController.IsAdminOrOwner(User, id.ToString()))
                return StatusCode(403);

            if (!_korisniciRepositrory.KorisnikExists(id))
                return NotFound();


            if (!_korisniciRepositrory.KorisnikExists(id))
                return NotFound();

            var korisnici = _mapper.Map<KorisniciDto>(_korisniciRepositrory.GetKorisnikId(id));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(korisnici);
        }

        [HttpGet("email/{email}")]
        public IActionResult GetKorisnik(string email)
        {
            var korisnik = _korisniciRepositrory.GetByEmail(email);

            if (korisnik == null)
                return NotFound();

            var dto = _mapper.Map<KorisniciDto>(korisnik);

            return Ok(dto);
        }

        [HttpGet("Profil/{id}")]
        [ProducesResponseType(403)]
        public IActionResult GetInfo(int id)     //podaci o profilu when requsted by client
        {
            var korisnik = _korisniciRepositrory.GetKorisnikId(id);
            if (korisnik == null) return NotFound();

            if (korisnik.tip_korisnika != Tip_korisnika.Majstor &&
        korisnik.tip_korisnika != Tip_korisnika.Firma)
            {
                return Forbid();
            }

            if (korisnik.tip_korisnika == Tip_korisnika.Majstor)
            {
                var majstor = _majstoriRepository.GetMajstorId(id);

                var dto = new MajstoriDto
                {
                    prosek_ocena = majstor.prosek_ocena,
                    kraj_radnog_vremena = majstor.kraj_radnog_vremena,
                    pocetak_radnog_vremena = majstor.pocetak_radnog_vremena,
                    broj_recenzija = majstor.broj_recenzija,
                    opis_usluga = majstor.opis_usluga
                };
                return Ok(dto);
            }

            if (korisnik.tip_korisnika == Tip_korisnika.Firma)
            {
                var firma = _firmeRepository.GetFirmuId(id);

                var dto = new FirmeDto
                {
                    naziv_firme = firma.naziv_firme,
                    broj_dostupnih_radnika = firma.broj_dostupnih_radnika,
                    prosek_ocena = firma.prosek_ocena,
                    kraj_radnog_vremena = firma.kraj_radnog_vremena,
                    pocetak_radnog_vremena = firma.pocetak_radnog_vremena,
                    broj_recenzija = firma.broj_recenzija,
                    opis_usluga = firma.opis_usluga
                };
                return Ok(dto);
            }
            return BadRequest(403);

        }



        [HttpPost("Create")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateKorisnik([FromBody] RegisterKorisnikDto dto)
        {

            if (dto == null)
                return BadRequest("Dto is empty");
            /*
            if(dto.tip_korisnika==Tip_korisnika.Admin)
                return Unauthorized();
            */
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_korisniciRepositrory.GetByEmail(dto.email) != null)
                return StatusCode(422, ModelState);
            //return BadRequest("NE valja mejl");    

            var transaction = _context.Database.BeginTransaction();

            try
            {
                if (dto.tip_korisnika == Tip_korisnika.Majstor)
                {
                    if (dto.pocetakRV > dto.krajRV)
                    {
                        transaction.Rollback();
                        return BadRequest("Invalidni podaci");
                    }
                    if (dto.kategorijeIds != null && dto.kategorijeIds.Any(id => id == 0 || id == 1))
                    {
                        transaction.Rollback();
                        return BadRequest("Kategorije ne smeju biti 0 ili 1");
                    }
                    if (!dto.pocetakRV.HasValue || !dto.krajRV.HasValue)
                    {
                        transaction.Rollback();
                        return BadRequest("Radno vreme ne sme biti null");
                    }

                }
                if (dto.tip_korisnika == Tip_korisnika.Firma)
                {
                    if (string.IsNullOrEmpty(dto.naziv))
                    {
                        transaction.Rollback();
                        return BadRequest("Naziv firme je obavezan");
                    }
                    if (dto.brDostupnih > dto.brUkupni || dto.pocetakRV > dto.krajRV)
                    {
                        transaction.Rollback();
                        return BadRequest("Invalidni podaci");
                    }
                    if (dto.kategorijeIds != null && dto.kategorijeIds.Any(id => id == 0 || id == 1))
                    {
                        transaction.Rollback();
                        return BadRequest("Kategorije ne smeju biti 0 ili 1");
                    }
                    if (!dto.pocetakRV.HasValue || !dto.krajRV.HasValue)
                    {
                        transaction.Rollback();
                        return BadRequest("Radno vreme ne sme biti null");
                    }
                }

                var hasher = new PasswordHasher<korisnici>();
                var korisnik = new korisnici
                {
                    ime = dto.ime,
                    prezime = dto.prezime,
                    email = dto.email,
                    lozinka = "",
                    tip_korisnika = dto.tip_korisnika,
                    telefon = dto.telefon,
                    datum_registacije = DateTime.Now
                };
                korisnik.lozinka = hasher.HashPassword(korisnik, dto.lozinka);

                if (!_korisniciRepositrory.CreateKorisnik(korisnik))
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Greska pri pravljenju korisnika");
                    return StatusCode(500, ModelState);
                }
                if (korisnik.tip_korisnika == Tip_korisnika.Firma)
                {
                    var firma = new firme
                    {
                        id_firme = korisnik.id_korisnik,
                        broj_ukupnih_radnika = dto.brUkupni,
                        broj_dostupnih_radnika = dto.brDostupnih,
                        naziv_firme = dto.naziv,
                        pocetak_radnog_vremena = dto.pocetakRV.Value,
                        kraj_radnog_vremena = dto.krajRV.Value
                    };
                    if (dto.kategorijeIds != null && dto.kategorijeIds.Any())
                    {
                        var pripade = dto.kategorijeIds.Select(k => new pripada
                        {
                            id_izvodjaca = korisnik.id_korisnik,
                            id_kategorije = k
                        }).ToList();

                        if (!_kategorijeRepository.KategorijePostoje(dto.kategorijeIds))
                        {
                            transaction.Rollback();
                            return BadRequest("Kategory invalid");
                        }

                        if (!_pripadaRepository.AddPripad(pripade))
                        {
                            transaction.Rollback();
                            return BadRequest("Kategory invalid");
                        }
                    }

                    if (!_firmeRepository.CreateFirmu(firma))
                    {
                        transaction.Rollback();
                        return BadRequest("Problem while creating profile");
                    }
                }

                if (korisnik.tip_korisnika == Tip_korisnika.Majstor)
                {
                    var majstor = new majstori
                    {
                        id_majstora = korisnik.id_korisnik,
                        pocetak_radnog_vremena = dto.pocetakRV.Value,
                        kraj_radnog_vremena = dto.krajRV.Value,

                    };
                    if (!_majstoriRepository.CreateMajstor(majstor))
                    {
                        transaction.Rollback();
                        return BadRequest("Problem while creating profile\"");
                    }
                    if (dto.kategorijeIds != null && dto.kategorijeIds.Any())
                    {
                        var pripade = dto.kategorijeIds.Select(k => new pripada
                        {
                            id_izvodjaca = korisnik.id_korisnik,
                            id_kategorije = k
                        }).ToList();

                        if (!_kategorijeRepository.KategorijePostoje(dto.kategorijeIds))
                        {
                            transaction.Rollback();
                            return BadRequest("Kategory invalid");
                        }

                        if (!_pripadaRepository.AddPripad(pripade))
                        {
                            transaction.Rollback();
                            return BadRequest("Kategory invalid");
                        }
                    }
                }

                if (korisnik.tip_korisnika == Tip_korisnika.Klijent)
                {

                    var klijent = new klijenti
                    {
                        id_klijent = korisnik.id_korisnik
                    };
                    if (!_klijentiRepository.CreateKlijenti(klijent))
                    {
                        transaction.Rollback();
                        return BadRequest("Problem while creating profile\"");
                    }
                }


                string token = _tokenProvider.Create(korisnik);

                transaction.Commit();

                return Ok(token); //ne treba da vraca korisnik ng ja da vidim sta radi
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException?.Message);

                return BadRequest("Problem while creating profile");
            }
        }

        [HttpPost("LoginUser")]
        public IActionResult LoginUser([FromQuery] string Email, [FromQuery] string lozinka) //LOgin za korisnike
        {
            var korisnik = _korisniciRepositrory.GetByEmail(Email);
            if (korisnik == null)
                return Ok(false);

            if (korisnik.blokiran == true)
                return BadRequest("User has been blocked");

            var hasher = new PasswordHasher<korisnici>();

            var result = hasher.VerifyHashedPassword(korisnik, korisnik.lozinka, lozinka);

            if (result != PasswordVerificationResult.Success)
            {
                return Ok(false);
            }

            string token = _tokenProvider.Create(korisnik);

            return Ok(token);
        }


        [Authorize]
        [HttpDelete("DeleteUser")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteKorisnik()      //Brisanje korisnika 
        {
            /*
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }
            */
            int korisnikId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out korisnikId))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsAdminOrOwner(User, korisnikId.ToString()))
                return StatusCode(403);

            if (!_korisniciRepositrory.KorisnikExists(korisnikId))
            {
                return NotFound();
            }

            var korisnikToDelete = _korisniciRepositrory.GetKorisnikId(korisnikId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_korisniciRepositrory.DeleteKorisnik(korisnikToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting Grad");
                return BadRequest(ModelState);
            }

            return Ok("deleted");

        }

        [Authorize]
        [HttpPut("SetSlika")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddSlika([FromForm] SlikaDto dto) 
        {


            int korisnikId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out korisnikId))
            {
                return Unauthorized();
            }

            if (!AuthorizationController.IsAdminOrOwner(User, korisnikId.ToString()))
                return StatusCode(403);

            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            var slika = dto.slika;

            if (slika == null)
                return BadRequest("Slika format invalid");

            var korisnik = _korisniciRepositrory.GetKorisnikId(korisnikId);

            if(korisnik==null)
                return Unauthorized();

            if(!AuthorizationController.IsSlika(slika))
                return BadRequest("Not a slika");

            using (var ms = new MemoryStream())
            {
                await slika.CopyToAsync(ms);
                korisnik.profilna_slika = ms.ToArray();
            }

            if (!_korisniciRepositrory.UpdateKorisnik(korisnik)) 
                return BadRequest("Something went wrong while saving");

            return Ok("Uspeno promenjena profilna slika");
        }


        [HttpGet("GetSlika")]
        public IActionResult GetSlika()
        {

            int korisnikId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out korisnikId))
            {
                return Unauthorized();
            }

            var korisnik = _korisniciRepositrory.GetKorisnikId(korisnikId);

            if (korisnik == null || korisnik.profilna_slika == null)
                return NotFound();

            return Ok(new
                    { 
                image = Convert.ToBase64String(korisnik.profilna_slika)
                    });
        }

        [Authorize]
        [HttpGet("MakeStripeNalog")]
        public IActionResult MakeNalog()
        {
            int korisnikId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);

            if (!int.TryParse(claim, out korisnikId))
            {
                return Unauthorized();
            }

            var izvodjac = _korisniciRepositrory.GetKorisnikId(korisnikId);

            if (izvodjac == null)
                return BadRequest("Nema korisnika");

            if (izvodjac.tip_korisnika != Tip_korisnika.Majstor &&
                izvodjac.tip_korisnika != Tip_korisnika.Firma)
                return Unauthorized();
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var options = new AccountCreateOptions
            {
                Type = "express",
                Country = "BG",
                Email = izvodjac.email,
            };

            var service = new AccountService();
            var account = service.Create(options);

            izvodjac.stripe_num = account.Id;

            if (!_korisniciRepositrory.UpdateKorisnik(izvodjac))
                return BadRequest("Problem creating account");

            var linkService = new AccountLinkService();

            var link = linkService.Create(new AccountLinkCreateOptions
            {
                Account = account.Id,
                 RefreshUrl = "http://127.0.0.1:5500/wwwroot/pages/profil.html",
                ReturnUrl = "http://127.0.0.1:5500/wwwroot/pages/profil.html",
                Type = "account_onboarding"
            });

            return Ok(new
            {
                stripeAccountId = account.Id,
                onboardingUrl = link.Url
            });
        }
        //TO-DO treba na front da ga posaljes na link da napravi nallog

    }
}
/*
 using var context = new MyDbContext();
// Open the database transaction explicitly
using var transaction = await context.Database.BeginTransactionAsync();

try
{
    context.Users.Add(new User { Name = "Alice" });
    await context.SaveChangesAsync();

    context.Logs.Add(new Log { Message = "User added" });
    await context.SaveChangesAsync();

    // Commit all operations if everything succeeds
    await transaction.CommitAsync();
}
catch (Exception)
{
    // Roll back changes if an exception occurs
    await transaction.RollbackAsync();
}
*/
