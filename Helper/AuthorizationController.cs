using Majstor_bob.Enums;
using System.Security.Claims;
using SixLabors.ImageSharp;
using static System.Net.Mime.MediaTypeNames;


namespace Majstor_bob.Helper
{
    public static class AuthorizationController
    {
        public static bool IsAdminOrOwner(ClaimsPrincipal user, string userId)
        {
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            var id = user.FindFirst(ClaimTypes.Sid)?.Value;

            return role == "Admin" || id == userId;
        }

        public static bool IsKlijent(ClaimsPrincipal user)
        {
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            return role == "Klijent";
        }

        public static bool IsAdmin(ClaimsPrincipal user)
        {
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            return role == "Admin";
        }

        public static bool IsMajstor(ClaimsPrincipal user)
        {
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            return role == "Majstor";
        }

        public static bool IsFirma(ClaimsPrincipal user)
        {
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            return role == "Firma";
        }

        public static bool IsJmbg(string jmbg)
        {
            if (string.IsNullOrEmpty(jmbg) || jmbg.Length != 13 || !jmbg.All(char.IsDigit))
                return false;

            int[] w = { 7, 6, 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

            int suma = 0;

            for (int i = 0; i < 12; i++)
                suma += (jmbg[i] - '0') * w[i];

            int mod = suma % 11;
            int kontrola = 11 - mod;

            if (kontrola == 11) kontrola = 0;
            if (kontrola == 10) return false;

            return kontrola == (jmbg[12] - '0');
        }

       

        public static bool isPIB(string pib)
        {
            if (string.IsNullOrEmpty(pib) || pib.Length != 9 || !pib.All(char.IsDigit))
                return false;

            int m = 10;
            int s = 0;
            for (int i = 0; i < 8; i++)
            {
                int cifra = pib[i] - '0';
                s = (cifra + m) % 10;
                    if (s == 0)
                    s = 10;
                m = (2 * s) % 11;
            }
            int k = (11 - m) % 10;
            if (k != pib[8]-'0')
                return false;

            return true;
        }

        /*
         ДД - дан рођења
    ММ - месец рођења
    ГГГ - задње три цифре године рођења
    РР - регион рођења или пребивалишта
    БББ - јединствени број
    K - контролна цифра
         */



        public static bool IsSlika(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            string[] dozvoljeniTipovi =
            {
        "image/jpeg",
        "image/png"
        };

            if (!dozvoljeniTipovi.Contains(file.ContentType))
                return false;

            string ekstenzija = Path.GetExtension(file.FileName).ToLower();

            string[] dozvoljeneEkstenzije =
            {
        ".jpg",
        ".jpeg",
        ".png"
        };

            if (!dozvoljeneEkstenzije.Contains(ekstenzija))
                return false;

            try
            {
                using var stream = file.OpenReadStream();
                using var image = SixLabors.ImageSharp.Image.Load(stream);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}     

