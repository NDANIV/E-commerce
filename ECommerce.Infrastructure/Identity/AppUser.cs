using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.Identity;

/// <summary>
/// Usuario de la aplicación basado en ASP.NET Identity con clave primaria Guid.
/// Extiende <see cref="IdentityUser{TKey}"/> para permitir agregar propiedades
/// de perfil en el futuro (p. ej. Nombre, Dirección, etc.).
/// </summary>
public class AppUser : IdentityUser<Guid>
{
    // TODO: Agregar propiedades de perfil (FirstName, LastName, etc.) cuando las necesites.
}