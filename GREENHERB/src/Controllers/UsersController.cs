using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GREENHERB.src.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAll()
    {
        _logger.LogInformation("Obtendo todos os utilizadores");
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetById([FromRoute] int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Tentativa de obter utilizador com ID inválido: {id}", id);
            return BadRequest(new { message = "Id inválido" });
        }

        var user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Utilizador com ID {id} não encontrado", id);
            return NotFound(new { message = "Utilizador não encontrado" });
        }

        return Ok(user);
    }

    [HttpGet("username/{username}")]
    public async Task<ActionResult<User>> GetByUsername([FromRoute] string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(new { message = "Username é obrigatório" });
        }

        var user = await _userService.GetByUsernameAsync(username);
        if (user == null)
        {
            _logger.LogWarning("Utilizador com username {username} não encontrado", username);
            return NotFound(new { message = "Utilizador não encontrado" });
        }

        return Ok(user);
    }

    [HttpGet("role/{role}")]
    public async Task<ActionResult<IEnumerable<User>>> GetByRole([FromRoute] UserRole role)
    {
        _logger.LogInformation("Obtendo utilizadores com perfil: {role}", role);
        var users = await _userService.GetByRoleAsync(role);
        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<User>> Create([FromBody] User user)
    {
        var validationError = ValidateUser(user);
        if (validationError != null)
        {
            _logger.LogWarning("Tentativa de criar utilizador com validação falha: {error}", validationError);
            return BadRequest(new { message = validationError });
        }

        _logger.LogInformation("Criando novo utilizador: {username}", user.Username);
        var created = await _userService.CreateAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<User>> Update([FromRoute] int id, [FromBody] User updated)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        var validationError = ValidateUser(updated);
        if (validationError != null)
        {
            _logger.LogWarning("Tentativa de atualizar utilizador com validação falha: {error}", validationError);
            return BadRequest(new { message = validationError });
        }

        _logger.LogInformation("Atualizando utilizador com ID: {id}", id);
        var updatedUser = await _userService.UpdateAsync(id, updated);
        if (updatedUser == null)
        {
            return NotFound(new { message = "Utilizador não encontrado" });
        }

        return Ok(updatedUser);
    }

    [HttpPut("{id:int}/change-role")]
    public async Task<ActionResult<User>> ChangeRole([FromRoute] int id, [FromBody] ChangeRoleRequest request)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        if (request?.NewRole == null)
        {
            return BadRequest(new { message = "Novo perfil é obrigatório" });
        }

        _logger.LogInformation("Alterando perfil do utilizador {id}", id);
        var user = await _userService.ChangeRoleAsync(id, request.NewRole);
        if (user == null)
        {
            return NotFound(new { message = "Utilizador não encontrado" });
        }

        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        _logger.LogInformation("Deletando utilizador com ID: {id}", id);
        var deleted = await _userService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Utilizador não encontrado" });
        }

        return NoContent();
    }

    private static string? ValidateUser(User user)
    {
        if (user == null)
        {
            return "Dados do utilizador são obrigatórios";
        }

        if (string.IsNullOrWhiteSpace(user.Username))
        {
            return "Username é obrigatório";
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return "Email é obrigatório";
        }

        if (!user.Email.Contains("@"))
        {
            return "Email inválido";
        }

        if (string.IsNullOrWhiteSpace(user.FullName))
        {
            return "Nome completo é obrigatório";
        }

        return null;
    }
}

public class ChangeRoleRequest
{
    public UserRole NewRole { get; set; }
}
