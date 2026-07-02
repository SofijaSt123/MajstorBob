namespace Majstor_bob.Models
{
    public class zapisnik_admin
    {
        public int id_zapisnik { get; set;}
        public int id_admina { get; set;}
        public string ip_adresa { get; set;}
        public string akcija { get; set;}
        public DateTime datum {  get; set;}
        public korisnici admin { get; set;}

            
    }
}
