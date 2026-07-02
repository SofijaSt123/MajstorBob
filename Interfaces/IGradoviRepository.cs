using Majstor_bob.Models;

namespace Majstor_bob.Interfaces
{
    public interface IGradoviRepository
    {
        ICollection<gradovi> GetGradovi();
        gradovi GetGradoviId(int id);
        bool CreateGrad(gradovi grad);
        void UpdateGrad(gradovi grad);
        bool DeleteGrad(gradovi grad);
        bool GradExists(int id);
        public bool PrvoSlovoVeliko(string tekst);
        bool GradExistsNaziv(string ime);
        bool Save();
    }
}
