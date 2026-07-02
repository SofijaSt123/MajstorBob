namespace Majstor_bob.Models
{
    public class poruke
    {
        public int id_poruke { get; set; }
        public int id_razgovora { get; set; }
        public int posiljac_id { get; set; }
        public string tekst {  get; set; }
        public DateTime vreme {  get; set; }
        public razgovor razgovor { get; set; }
        public korisnici posiljac { get; set; }
    
    
    
    
    }
}
