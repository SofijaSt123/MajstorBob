using Majstor_bob.Enums;

namespace Majstor_bob.Dto
{
    public class KorisniciDto
    {
        public string email { get; set; }
        public bool blokiran { get; set; }
        public bool is_flaged { get; set; }
        public string ime { get; set; }
        public string prezime { get; set; }
        public Tip_korisnika tip_korisnika { get; set; } //enum    }
        public MajstoriDto? majstor { get; set; }
        public FirmeDto? firme { get; set; }
    }
}