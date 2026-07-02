using Majstor_bob.Dto;
using Majstor_bob.Enums;
using Majstor_bob.Models;

namespace Majstor_bob.Interfaces
{
    public interface IRazgovorRepository
    { 
        razgovor GetRazgovorId(int id);

        ICollection<GetRazgovorDto> GetRazgovoreByUserId(int userId, Tip_korisnika tip);
        bool CreateRazgovor(razgovor razgovor);
        bool UpdateRazgovor(razgovor razgovor);
        bool DeleteRazgovor(razgovor razgovor);
        bool RazgovorExists(int id);

        razgovor GetRazgovorByIds(int idUser1, int idUser2);


        bool RazgovorExists(int idUser1, int idUser2);
        bool Save();
    }
}
