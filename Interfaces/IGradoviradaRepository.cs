using Majstor_bob.Models;

namespace Majstor_bob.Interfaces
{
    public interface IGradoviradaRepository
    {
        ICollection<gradovi_rada> GetGradovirada();

        ICollection<gradovi_rada> GetGradoveIzvodjaca(int id_izvodjaca);
        ICollection<korisnici> GetIzvodjacByGrad(int gradId,bool doplata);

        gradovi_rada GetGradRadaId(int id);

        bool CreateGradRada(gradovi_rada grad);

        void UpdateGradRada(gradovi_rada grad);

        bool DeleteGradRada(gradovi_rada grad);

        bool GradRadaExists(int id);

        bool Save();
    }
}
