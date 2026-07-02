using Majstor_bob.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Majstor_bob.Dto
{
    public class RegisterKorisnikDto
    {
        [Required]
        [RegularExpression(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        ErrorMessage = "Neispravna email adresa."
        )]
        public string email { get; set; }
        
        [Required]
        [MinLength(8)]

        //[Required @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
       //ErrorMessage = "Lozinka mora imati najmanje 8 karaktera, jedno veliko slovo, jedno malo slovo i jedan broj."
        //)]
        public string lozinka { get; set; }
        public string ime { get; set; }
        public string prezime { get; set; }
        public Tip_korisnika tip_korisnika { get; set; } //enum    }

        public string? telefon { get; set; }

        public TimeOnly? pocetakRV { get; set; }
        public TimeOnly? krajRV { get; set; }
        public string? naziv { get; set; }
        public int brUkupni { get; set; }
        public int brDostupnih { get; set; }
        public List<int>? kategorijeIds { get; set; }
    }
}
