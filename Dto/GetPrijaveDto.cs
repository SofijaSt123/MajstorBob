namespace Majstor_bob.Dto
{
    public class GetPrijaveDto
    {
   
            public int id_prijave { get; set; }
            public int id_prijavljena_osoba { get; set; }
            public int id_prijavljaca { get; set; }
            public string razlog { get; set; }
            public string? admin_komentar { get; set; }
            public DateTime kreirano { get; set; }

        
    }
}
