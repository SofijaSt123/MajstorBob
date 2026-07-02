namespace Majstor_bob.Models
{
    public class gradovi_rada
    {
        public int id_grad_rada { get; set; }
        public int id_izvodjaca { get; set; }  
        public int id_grada {  get; set; }
        public bool zona_rada { get; set; }
        public int doplata { get; set; }

        public korisnici Izvodjac { get; set; } //1
        public gradovi grad { get; set; } //1
    }
}
