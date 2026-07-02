using Majstor_bob.Enums;

namespace Majstor_bob.Models
{
    public class obavestenja
    { 
            public int id { get; set; }
            public int admin_id { get; set; } 
            public korisnici sender { get; set; }
            public int? receiver_id { get; set; } 
            public korisnici receiver { get; set; }
            public string tip { get; set; }
            public string naslov { get; set; }
            public bool procitanno { get; set; }
            public Tip_obavestenja? tip_kome_saljes { get; set; } 
        
    }
}
/*
 za 1 korisnika
reciver_id=1,Tip_obavestenja=null
reciver_id=null,tipOvabestenja=Klijent...
 */