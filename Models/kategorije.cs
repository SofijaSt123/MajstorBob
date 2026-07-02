namespace Majstor_bob.Models
{
    public class kategorije
    {
        public int id_kategorije { get; set; }
        public int? id_roditelja { get; set; }
        public string naziv_kategorije { get; set; }
        public kategorije roditelj {  get; set; }
        public ICollection<kategorije> pod_kategorija { get; set; }
        public ICollection<pripada> kategorije_izvodjaca { get; set; }

    }
}
