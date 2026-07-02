using Majstor_bob.Models;

namespace Majstor_bob.Dto
{
    public class GetPorukeDto
    {
        public int id_poruke { get; set; }
        public int id_razgovora { get; set; }
        public int posiljac_id { get; set; }
        public string tekst { get; set; }
        public DateTime vreme { get; set; }
    }
}
