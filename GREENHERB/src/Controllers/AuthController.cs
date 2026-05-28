using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GREENHERB.src.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] AuthRequest request)
    {
        // Validações básicas
        if (request == null)
        {
            _logger.LogWarning("Tentativa de login com request nulo");
            return BadRequest(new { message = "Request não pode ser nulo" });
        }

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Tentativa de login com credenciais vazias");
            return BadRequest(new { message = "Username e password são obrigatórios" });
        }

        var result = await _authService.AuthenticateAsync(request.Username, request.Password);

        if (result == null)
        {
            _logger.LogWarning($"Falha na autenticação para utilizador: {request.Username}");
            return Unauthorized(new { message = "Credenciais inválidas" });
        }

        _logger.LogInformation($"Utilizador {request.Username} autenticado com sucesso");
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(
        [FromQuery] string username,
        [FromQuery] string email,
        [FromQuery] string password,
        [FromQuery] UserRole role)
    {
        // Validar entrada
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Tentativa de registo com dados vazios");
            return BadRequest(new { message = "Username, email e password são obrigatórios" });
        }

        var result = await _authService.RegisterAsync(username, email, password, role);

        if (result == null)
        {
            _logger.LogWarning($"Falha no registo para utilizador: {username}");
            return Conflict(new { message = "Falha no registo - verifique os dados fornecidos" });
        }

        _logger.LogInformation($"Novo utilizador registado: {username}");
        return CreatedAtAction(nameof(Register), new { id = result.Id }, result);
    }

    [HttpPost("validate")]
    public ActionResult<object> ValidateToken([FromBody] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { message = "Token é obrigatório" });
        }

        var isValid = _authService.ValidateToken(token);

        if (!isValid)
        {
            _logger.LogWarning("Tentativa de validar token inválido");
            return Unauthorized(new { message = "Token inválido ou expirado" });
        }

        var claims = _authService.GetTokenClaims(token);
        return Ok(new { isValid = true, claims });
    }
}
