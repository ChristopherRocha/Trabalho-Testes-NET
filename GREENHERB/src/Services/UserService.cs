using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using Microsoft.EntityFrameworkCore;

namespace GREENHERB.src.Services;

public class UserService : IUserService
{
    // Comentar a linha abaixo para usar BD real; descomente para usar mock
    private readonly AppDbContext? _context;
    private readonly ILogger<UserService>? _logger;
    private bool _useMock = true;

    public UserService(AppDbContext? context = null, ILogger<UserService>? logger = null)
    {
        _context = context;
        _logger = logger;
        _useMock = context == null; // Usa mock se context é null
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        _logger?.LogInformation("Obtendo todos os utilizadores");
        if (_useMock)
            return MockDataProvider.GetAllUsers();

        return await _context!.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        _logger?.LogInformation("Obtendo utilizador com ID: {id}", id);
        if (_useMock)
            return MockDataProvider.GetUserById(id);

        return await _context!.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        _logger?.LogInformation("Obtendo utilizador com username: {username}", username);
        if (_useMock)
            return MockDataProvider.GetUserByUsername(username);

        return await _context!.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
    {
        _logger?.LogInformation("Obtendo utilizadores com perfil: {role}", role);
        if (_useMock)
            return MockDataProvider.GetAllUsers().Where(u => u.Role == role);

        return await _context!.Users.Where(u => u.Role == role).ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _logger?.LogInformation("Criando novo utilizador: {username}", user.Username);
        if (_useMock)
        {
            var result = MockDataProvider.AddUser(user);
            _logger?.LogInformation("Utilizador criado com sucesso. ID: {id}", result.Id);
            return result;
        }

        _context!.Users.Add(user);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Utilizador criado com sucesso. ID: {id}", user.Id);
        return user;
    }

    public async Task<User?> UpdateAsync(int id, User user)
    {
        _logger?.LogInformation("Atualizando utilizador com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.UpdateUser(id, user);
            if (result == null)
                _logger?.LogWarning("Utilizador com ID {id} não encontrado", id);
            else
                _logger?.LogInformation("Utilizador atualizado com sucesso");
            return result;
        }

        var existing = await _context!.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (existing == null)
        {
            _logger?.LogWarning("Utilizador com ID {id} não encontrado", id);
            return null;
        }

        existing.Username = user.Username;
        existing.Email = user.Email;
        existing.FullName = user.FullName;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger?.LogInformation("Utilizador atualizado com sucesso");
        
        return existing;
    }

    public async Task<User?> ChangeRoleAsync(int id, UserRole newRole)
    {
        _logger?.LogInformation("Alterando perfil do utilizador {id} para {role}", id, newRole);
        
        if (_useMock)
        {
            var user = MockDataProvider.GetUserById(id);
            if (user == null)
            {
                _logger?.LogWarning("Utilizador com ID {id} não encontrado", id);
                return null;
            }
            user.Role = newRole;
            user.UpdatedAt = DateTime.UtcNow;
            _logger?.LogInformation("Perfil do utilizador alterado com sucesso");
            return user;
        }

        var userDb = await _context!.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (userDb == null)
        {
            _logger?.LogWarning("Utilizador com ID {id} não encontrado", id);
            return null;
        }

        userDb.Role = newRole;
        userDb.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Perfil do utilizador alterado com sucesso");
        return userDb;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger?.LogInformation("Deletando utilizador com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.DeleteUser(id);
            if (!result)
                _logger?.LogWarning("Utilizador com ID {id} não encontrado", id);
            else
                _logger?.LogInformation("Utilizador deletado com sucesso");
            return result;
        }

        var user = await _context!.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            _logger?.LogWarning("Utilizador com ID {id} não encontrado", id);
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Utilizador deletado com sucesso");
        return true;
    }
}
