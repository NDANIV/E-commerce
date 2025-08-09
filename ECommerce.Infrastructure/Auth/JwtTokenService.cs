using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Auth;

/// <summary>
/// Contrato para la emisión de tokens JWT.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Crea un token JWT firmado con los datos del usuario y sus roles.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="email">Correo del usuario (se incluye como claim).</param>
    /// <param name="roles">Colección de roles del usuario.</param>
    /// <returns>Cadena JWT serializada.</returns>
    Task<string> CreateTokenAsync(Guid userId, string email, IEnumerable<string> roles);
}

/// <summary>
/// Implementación por defecto de <see cref="IJwtTokenService"/> que
/// lee configuración desde "Jwt:*" en el <see cref="IConfiguration"/>.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _cfg;

    /// <summary>
    /// Inyecta la configuración de la aplicación.
    /// </summary>
    public JwtTokenService(IConfiguration cfg) => _cfg = cfg;

    /// <inheritdoc/>
    public Task<string> CreateTokenAsync(Guid userId, string email, IEnumerable<string> roles)
    {
        var issuer = _cfg["Jwt:Issuer"] ?? "ECommerce";
        var audience = _cfg["Jwt:Audience"] ?? "ECommerce";
        var keyString = _cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurado.");
        var expiresHours = int.TryParse(_cfg["Jwt:ExpiresHours"], out var h) ? h : 2;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(expiresHours),
            signingCredentials: creds
        );

        var serialized = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(serialized);
    }
}
