using MonetixFogachoReveloAPI.Data.Models;
using System.Security.Claims;

namespace MonetixFogachoReveloAPI.Interfaz
{
    public interface ITokenService
    {
        string GenerateJwtToken(Usuario usuario);
        string GenerateJwtToken(Claim[] claims);
    }
}
