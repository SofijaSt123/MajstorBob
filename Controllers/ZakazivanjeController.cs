using AutoMapper;
using Majstor_bob.Helper;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using static System.Net.WebRequestMethods;
using Majstor_bob.Enums;

namespace Majstor_bob.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZakazivanjeController : Controller
    {

        private readonly IZakazivanjeRepository _zakazivanjeRepository;
        private readonly IMapper _mapper;
        private readonly IKorisniciRepository _korisniciRepository;
        private readonly StripeSettings _stripeSettings;
        public ZakazivanjeController(IZakazivanjeRepository zakazivanjeRepository, IMapper mapper, IKorisniciRepository korisniciRepository
            , IOptions<StripeSettings> stripeSettings)
        {
            _zakazivanjeRepository = zakazivanjeRepository;
            _mapper = mapper;
            _korisniciRepository = korisniciRepository;
            _stripeSettings = stripeSettings.Value;
        }

        [Authorize]
        [HttpPut("PlacanjeKesom/{id_zakazivanje}")]

        public IActionResult Plati([FromQuery] int konacna_cena, int id_zakazivanje)
        {
            int izvodjacId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out izvodjacId))
            {
                return Unauthorized();
            }

            var izvodjac = _korisniciRepository.GetKorisnikId(izvodjacId);

            if (izvodjac == null)
                return Unauthorized();

            var zakaz = _zakazivanjeRepository.GetZakazivanjeId(id_zakazivanje);

            if (zakaz == null)
                return BadRequest("Zakazivanje doesnt exist");

            if (zakaz.zahtev.id_izvodjaca != izvodjacId)
                return Unauthorized();

            var maxDozvoljena = zakaz.cena_gornja * 1.3;

            if (konacna_cena > maxDozvoljena)
                return BadRequest($"Cena ne sme biti veća od {maxDozvoljena}");


            zakaz.konacna_cena = konacna_cena;
            zakaz.da_li_je_placeno = true;
            if (!_zakazivanjeRepository.UpdateZakazivanje(zakaz))
                return BadRequest("Problem saving in database");

            return Ok("Uspesno je obavljeno placanje");

        }


        [Authorize]
        [HttpPut("SetKonacna/{id_zakazivanje}")]

        public IActionResult SetKonacna([FromRoute] int id_zakazivanje, [FromQuery] int konacna_cena)
        {
            int izvodjacId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out izvodjacId))
            {
                return Unauthorized();
            }

            var izvodjac = _korisniciRepository.GetKorisnikId(izvodjacId);

            if (izvodjac == null)
                return Unauthorized();

            var zakaz = _zakazivanjeRepository.GetZakazivanjeId(id_zakazivanje);

            if (zakaz == null)
                return BadRequest("Zakazivanje doesnt exist");

            if (zakaz.zahtev.id_izvodjaca != izvodjacId)
                return Unauthorized();

            var maxDozvoljena = zakaz.cena_gornja * 1.3;

            if (konacna_cena > maxDozvoljena)
                return BadRequest($"Cena ne sme biti veća od {maxDozvoljena}");

            zakaz.konacna_cena = konacna_cena;
            if (!_zakazivanjeRepository.UpdateZakazivanje(zakaz))
                return BadRequest("Problem saving in database");

            return Ok("Uspesno je postavljena konacna cena");

        }


        [Authorize]
        [HttpPost("CreateCheckoutSession")]
        public IActionResult CreateCheckoutSession(int id_zakazivanja)
        {
            int klijentId = 0;
            var claim = User.FindFirstValue(ClaimTypes.Sid);
            if (!int.TryParse(claim, out klijentId))
            {
                return Unauthorized();
            }

            var klijent = _korisniciRepository.GetKorisnikId(klijentId);
            if (klijent == null)
                return BadRequest("Klijent ne postoji");

            if (klijent.tip_korisnika!=Tip_korisnika.Klijent)
                return BadRequest("Ne mozes da platis");

          

            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var zakaz = _zakazivanjeRepository.GetZakazivanjeId(id_zakazivanja);

            
            if (zakaz == null)
                return BadRequest("Ne postoji zakazivanje");

            if (klijent.id_korisnik != zakaz.zahtev.id_klijenta)
                return BadRequest("Nije tvoje zakazivanje da platis");


            var majstor = _korisniciRepository.GetKorisnikId(zakaz.zahtev.id_izvodjaca);

            if (majstor == null)
                return BadRequest("Majstor ne postoji");

            if (string.IsNullOrEmpty(majstor.stripe_num))
                return BadRequest("Majstor nema Stripe Connect nalog");

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",

                SuccessUrl = "http://127.0.0.1:5500/wwwroot/pages/profil.html",
                CancelUrl = "http://127.0.0.1:5500/wwwroot/pages/profil.html",
                //ovde se menja url
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    TransferData = new SessionPaymentIntentDataTransferDataOptions
                    {
                        Destination = majstor.stripe_num
                    }
                },

                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "eur",
                    UnitAmount = (long)(zakaz.konacna_cena * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = $"Zakazivanje #{id_zakazivanja}"
                    }
                },
                Quantity = 1
            }
        },

                Metadata = new Dictionary<string, string>
        {
            { "zakazivanjeId", id_zakazivanja.ToString() },
            { "izvodjacId", majstor.id_korisnik.ToString() },
            { "klijentId", klijent.id_korisnik.ToString() }
        }
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Ok(new { url = session.Url });
        }

        [HttpPost("stripe-webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            Console.WriteLine("WEBHOOK HIT");

            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            var stripeSignature = Request.Headers["Stripe-Signature"];

            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                _stripeSettings.WebhookSecret
            );

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                if (session?.Metadata == null || !session.Metadata.ContainsKey("zakazivanjeId"))
                    return BadRequest("Missing metadata");

                var zakazId = session.Metadata["zakazivanjeId"];

                var zakaz = _zakazivanjeRepository.GetZakazivanjeId(int.Parse(zakazId));

                if (zakaz != null)
                {
                    zakaz.da_li_je_placeno = true;
                    _zakazivanjeRepository.UpdateZakazivanje(zakaz);
                }
            }

            return Ok();
        }



























    }
}
