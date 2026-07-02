namespace Majstor_bob.Models
{
    public class gradovi
    {
        public int id_grad {  get; set; }
        public string naziv_grada { get; set; }
        public int? postanski_broj { get; set; }

        public ICollection<gradovi_rada> gradovi_rad {  get; set; }

    }
}
