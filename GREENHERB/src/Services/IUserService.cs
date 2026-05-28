using GREENHERB.src.Models;

namespace GREENHERB.src.Services;

public interface IUserService
{
    /// <summary>
    /// Obtém todos os utilizadores.
    /// </summary>
    Task<IEnumerable<User>> GetAllAsync();

    /// <summary>
    /// Obtém um utilizador pelo ID.
    /// </summary>
    Task<User?> GetByIdAsync(int id);

    /// <summary>
    /// Obtém um utilizador pelo nome de utilizador.
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Obtém utilizadores por perfil.
    /// </summary>
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role);

    /// <summary>
    /// Cria um novo utilizador.
    /// </summary>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Atualiza um utilizador.
    /// </summary>
    Task<User?> UpdateAsync(int id, User user);

    /// <summary>
    /// Altera o perfil de um utilizador.
    /// </summary>
    Task<User?> ChangeRoleAsync(int id, UserRole newRole);

    /// <summary>
    /// Deleta um utilizador.
    /// </summary>
    Task<bool> DeleteAsync(int id);
}
