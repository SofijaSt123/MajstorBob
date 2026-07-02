using Majstor_bob.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Majstor_bob.Helper
{
    public sealed class TokenProvider(IConfiguration configuration)
    {
        public string Create(korisnici korisnik)
        {
            string secretKey = configuration["Jwt:SigningKey"]!;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var credentials= new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    [
                    new Claim(ClaimTypes.Sid,korisnik.id_korisnik.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email,korisnik.email),
                    new Claim(ClaimTypes.Role,korisnik.tip_korisnika.ToString())
                    ]),
                Expires = DateTime.UtcNow.AddMinutes(45),
                SigningCredentials = credentials,
                Issuer = configuration["Jwt:Issuer"],
                Audience = configuration["Jwt:Audience"]

            };
            var handelr = new JsonWebTokenHandler();
            string token =handelr.CreateToken(tokenDescriptor);
            return token;
        }

    }
}
