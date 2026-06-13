using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using GREENHERB.src.Models;
using GREENHERB.src.Data.Contexts;
using Microsoft.IdentityModel.Tokens;

namespace GREENHERB.src.Services;

public class AuthService : IAuthService
{
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpirationMinutes;
    private readonly Dictionary<string, User> _users; // Mock database (fallback)
    private readonly AppDbContext? _dbContext; // Database context for real users

    private const int MinUsernameLength = 3;
    private const int MaxUsernameLength = 50;
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 128;
    private const string EmailRegexPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

    public AuthService(
        IConfiguration configuration,
        AppDbContext? dbContext = null,
        Dictionary<string, User>? mockUsers = null)
    {
        _jwtKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException(nameof(configuration), "Jwt:Key is required");
        _jwtIssuer = configuration["Jwt:Issuer"] ?? "GREENHERB";
        _jwtAudience = configuration["Jwt:Audience"] ?? "GREENHERBUsers";
        _jwtExpirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");
        _users = mockUsers ?? new Dictionary<string, User>();
        _dbContext = dbContext;
    }

    // Compatibilidade com testes: sobrecarga que aceita dicionário de users como segundo parâmetro
    public AuthService(IConfiguration configuration, Dictionary<string, User> mockUsers)
        : this(configuration, null, mockUsers)
    {
    }

    public async Task<AuthResponse?> AuthenticateAsync(string username, string password)
    {
        // Validações de entrada
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        if (!ValidateUsernameLength(username))
            return null;

        if (!ValidatePasswordLength(password))
            return null;

        User? user = null;

        // Tentar buscar do banco de dados primeiro
        if (_dbContext != null)
        {
            user = await Task.FromResult(_dbContext.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower()));
        }

        // Fallback para dicionário mock se não encontrado no banco
        if (user == null && _users.ContainsKey(username.ToLower()))
        {
            user = _users[username.ToLower()];
        }

        // Se ainda não encontrou usuário, retornar null
        if (user == null)
            return null;

        // Validar se utilizador está ativo
        if (!user.IsActive)
            return null;

        // Verificar password
        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        // Gerar token
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtExpirationMinutes * 60,
            Username = user.Username,
            Role = user.Role
        };
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public Dictionary<string, string>? GetTokenClaims(string token)
    {
        if (!ValidateToken(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var claims = new Dictionary<string, string>();
            foreach (var claim in jwtToken.Claims)
            {
                claims[claim.Type] = claim.Value;
            }

            return claims;
        }
        catch
        {
            return null;
        }
    }

    public async Task<User?> RegisterAsync(string username, string email, string password, UserRole role)
    {
        // Validações
        if (!ValidateUsernameLength(username))
            return null;

        if (!ValidateEmailFormat(email))
            return null;

        if (!ValidatePasswordLength(password))
            return null;

        // Verificar se utilizador já existe
        if (_users.ContainsKey(username.ToLower()))
            return null;

        // Criar utilizador
        var passwordHash = HashPassword(password);
        var user = new User
        {
            Id = _users.Count + 1,
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _users[username.ToLower()] = user;
        return user;
    }

    public bool ValidatePasswordLength(string password)
    {
        return !string.IsNullOrWhiteSpace(password) &&
               password.Length >= MinPasswordLength &&
               password.Length <= MaxPasswordLength;
    }

    public bool ValidateUsernameLength(string username)
    {
        return !string.IsNullOrWhiteSpace(username) &&
               username.Length >= MinUsernameLength &&
               username.Length <= MaxUsernameLength;
    }

    public bool ValidateEmailFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            return Regex.IsMatch(email, EmailRegexPattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim("email", user.Email),
                new Claim("role", user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
            Issuer = _jwtIssuer,
            Audience = _jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(hash);
    }

    public async Task<string?> RenewTokenAsync(string token)
    {
        if (!ValidateToken(token))
            return null;

        try
        {
            var claims = GetTokenClaims(token);
            if (claims == null || !claims.ContainsKey("username"))
                return null;

            var username = claims["username"];
            User? user = null;

            // Tentar buscar do banco de dados primeiro
            if (_dbContext != null)
            {
                user = await Task.FromResult(_dbContext.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower()));
            }

            // Fallback para dicionário mock
            if (user == null && _users.ContainsKey(username.ToLower()))
            {
                user = _users[username.ToLower()];
            }

            if (user == null)
                return null;

            var newToken = GenerateJwtToken(user);
            return newToken;
        }
        catch
        {
            return null;
        }
    }
}
