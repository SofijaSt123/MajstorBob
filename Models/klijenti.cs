using System.ComponentModel.DataAnnotations;

namespace Majstor_bob.Models
{
    public class klijenti
    {
        [Key]
        public int id_klijent { get; set; }
        public korisnici korisnik { get; set; }
        public int? id_grad_rada { get; set; }
        public gradovi? gradGdeKlijentRad { get; set; }
        
        public ICollection<ocene> ocene { get; set; }
        public ICollection<prijave> ko_prijavljuje { get; set; }
        public ICollection<zahtevi> sender_zahteva {  get; set; }
        public ICollection<zakazivanje> kome_salje_zakaz {  get; set; }
        public ICollection<razgovor> razgovors { get; set; }

    }
}
