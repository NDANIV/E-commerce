using System.Security.Claims;

namespace ECommerce.WebApi.Extensions;

/// <summary>Ayudas para extraer el UserId (Guid) desde el JWT.</summary>
public static class UserExtensions
{
    /// <summary>Obtiene el Guid del usuario a partir del claim NameIdentifier o Sub.</summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? user.FindFirstValue("sub")
                 ?? throw new InvalidOperationException("No se encontró el identificador de usuario en el token.");
        return Guid.Parse(id);
    }
}
