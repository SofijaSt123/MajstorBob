using Majstor_bob.Enums;

namespace Majstor_bob.Dto
{
    public class GetKlijentInfoDto
    {
        public string ime { get; set; }
        public string prezime { get; set; }
        public string? telefon { get; set; }
        public Tip_korisnika tip_korisnika { get; set; } //enum
    }
}
