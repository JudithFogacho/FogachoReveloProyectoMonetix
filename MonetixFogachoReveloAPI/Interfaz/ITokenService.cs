using MonetixFogachoReveloAPI.Data.Models;

namespace MonetixFogachoReveloAPI.Interfaz
{
    public interface ITokenService
    {
        string GenerateJwtToken(Usuario usuario);
    }
}
