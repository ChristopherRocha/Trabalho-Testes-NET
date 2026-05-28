using GREENHERB.src.Models;

namespace GREENHERB.src.Services;

public interface IAuthService
{
    /// <summary>
    /// Autentica um utilizador e retorna um token JWT.
    /// </summary>
    /// <param name="username">Nome do utilizador</param>
    /// <param name="password">Palavra-passe do utilizador</param>
    /// <returns>AuthResponse com token se bem-sucedido, null se falhar</returns>
    Task<AuthResponse?> AuthenticateAsync(string username, string password);

    /// <summary>
    /// Valida um token JWT.
    /// </summary>
    /// <param name="token">Token a validar</param>
    /// <returns>True se o token é válido, false caso contrário</returns>
    bool ValidateToken(string token);

    /// <summary>
    /// Extrai as informações de utilizador de um token JWT.
    /// </summary>
    /// <param name="token">Token JWT</param>
    /// <returns>Dicionário com claims do token ou null se inválido</returns>
    Dictionary<string, string>? GetTokenClaims(string token);

    /// <summary>
    /// Regista um novo utilizador.
    /// </summary>
    /// <param name="username">Nome do utilizador (entre 3 e 50 caracteres)</param>
    /// <param name="email">Email do utilizador (formato válido)</param>
    /// <param name="password">Palavra-passe (mínimo 8 caracteres)</param>
    /// <param name="role">Perfil do utilizador</param>
    /// <returns>User criado se bem-sucedido, null se falhar</returns>
    Task<User?> RegisterAsync(string username, string email, string password, UserRole role);

    /// <summary>
    /// Valida o comprimento da palavra-passe.
    /// </summary>
    /// <param name="password">Palavra-passe a validar</param>
    /// <returns>True se a palavra-passe é válida</returns>
    bool ValidatePasswordLength(string password);

    /// <summary>
    /// Valida o comprimento do nome de utilizador.
    /// </summary>
    /// <param name="username">Nome de utilizador a validar</param>
    /// <returns>True se o nome é válido</returns>
    bool ValidateUsernameLength(string username);

    /// <summary>
    /// Valida o formato de email.
    /// </summary>
    /// <param name="email">Email a validar</param>
    /// <returns>True se o email é válido</returns>
    bool ValidateEmailFormat(string email);

    /// <summary>
    /// Renova um token JWT válido gerando um novo com as mesmas informações.
    /// </summary>
    /// <param name="token">Token a renovar</param>
    /// <returns>Novo token se o token atual for válido, null caso contrário</returns>
    Task<string?> RenewTokenAsync(string token);
}
