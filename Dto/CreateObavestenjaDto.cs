using Majstor_bob.Enums;
using Majstor_bob.Models;

namespace Majstor_bob.Dto
{
    public class CreateObavestenjaDto
    {
        public int? receiver_id { get; set; }
        public string naslov { get; set; }
        public Tip_obavestenja? tip_kome_saljes { get; set; }
    }
}
