using System.ComponentModel.DataAnnotations;
using ECommerce.Infrastructure.Auth;
using ECommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebApi.Controllers;

/// <summary>
/// Endpoints de autenticación y registro de usuarios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _users;
    private readonly RoleManager<IdentityRole<Guid>> _roles;
    private readonly IJwtTokenService _jwt;

    /// <summary>
    /// Crea una nueva instancia del controlador de autenticación.
    /// </summary>
    /// <param name="users">Manejador de usuarios de Identity.</param>
    /// <param name="roles">Manejador de roles de Identity.</param>
    /// <param name="jwt">Servicio emisor de tokens JWT.</param>
    public AuthController(
        UserManager<AppUser> users,
        RoleManager<IdentityRole<Guid>> roles,
        IJwtTokenService jwt)
    {
        _users = users;
        _roles = roles;
        _jwt = jwt;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema con rol por defecto <c>Customer</c>.
    /// </summary>
    /// <param name="request">Datos de registro: email y password.</param>
    /// <response code="200">Registro exitoso.</response>
    /// <response code="400">Errores de validación o usuario existente.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Garantiza existencia del rol "Customer" (por si no se hizo seed aún)
        if (!await _roles.RoleExistsAsync("Customer"))
            await _roles.CreateAsync(new IdentityRole<Guid>("Customer"));

        var user = new AppUser { UserName = request.Email, Email = request.Email };
        var result = await _users.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _users.AddToRoleAsync(user, "Customer");
        return Ok();
    }

    /// <summary>
    /// Autentica a un usuario y devuelve un token JWT firmado.
    /// </summary>
    /// <param name="request">Credenciales: email y password.</param>
    /// <returns>Token JWT para usar en el header Authorization.</returns>
    /// <response code="200">Autenticación exitosa; retorna el token.</response>
    /// <response code="401">Credenciales inválidas.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email);
        if (user is null || !await _users.CheckPasswordAsync(user, request.Password))
            return Unauthorized();

        var roles = await _users.GetRolesAsync(user);
        var token = await _jwt.CreateTokenAsync(user.Id, user.Email!, roles);
        return Ok(new LoginResponse(token));
    }
}

/// <summary>
/// Datos de registro de usuario (email y contraseña).
/// </summary>
public sealed class RegisterRequest
{
    /// <summary>Email del usuario. Debe ser único.</summary>
    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    /// <summary>Contraseña del usuario (mínimo 6 caracteres por configuración).</summary>
    [Required, MinLength(6)]
    public string Password { get; set; } = default!;
}

/// <summary>
/// Credenciales de acceso para solicitar un token JWT.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>Email del usuario.</summary>
    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    /// <summary>Contraseña del usuario.</summary>
    [Required]
    public string Password { get; set; } = default!;
}

/// <summary>
/// Respuesta que contiene el token JWT emitido.
/// </summary>
/// <param name="Token">Token JWT firmado; úsalo como "Bearer &lt;token&gt;".</param>
public sealed record LoginResponse(string Token);
