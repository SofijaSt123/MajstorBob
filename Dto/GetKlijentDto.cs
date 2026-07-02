using Majstor_bob.Enums;

namespace Majstor_bob.Dto
{
    public class GetKlijentDto
    {
        public string email { get; set; }
        public string ime { get; set; }
        public string prezime { get; set; }
        public string? telefon { get; set; }
        public Tip_korisnika tip_korisnika { get; set; } //enum
        public DateTime datum_registacije { get; set; }

    }
}
