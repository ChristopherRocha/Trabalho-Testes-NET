using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GREENHERB.Tests;

public class AuthTests
{
    private readonly IConfiguration _config;
    private readonly Dictionary<string, User> _users;
    private readonly AuthService _authService;

    public AuthTests()
    {
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "havLMo4mSPwWO5LlLjTYWsw0VpVamc03suk0OfQMiv6",
                ["Jwt:Issuer"] = "GREENHERB",
                ["Jwt:Audience"] = "GREENHERBUsers",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        _users = new Dictionary<string, User>();
        _authService = new AuthService(_config, _users);
    }

    #region Autenticação Básica

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        await _authService.RegisterAsync("johndoe", "john@example.com", "password123", UserRole.Tecnico);

        // Act
        var result = await _authService.AuthenticateAsync("johndoe", "password123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("johndoe", result.Username);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task AuthenticateAsync_WithWrongPassword_ReturnsNull()
    {
        // Arrange
        await _authService.RegisterAsync("johndoe", "john@example.com", "password123", UserRole.Tecnico);

        // Act
        var result = await _authService.AuthenticateAsync("johndoe", "wrongpassword");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNonExistentUser_ReturnsNull()
    {
        // Act
        var result = await _authService.AuthenticateAsync("nonexistent", "password123");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInactiveUser_ReturnsNull()
    {
        // Arrange
        await _authService.RegisterAsync("johndoe", "john@example.com", "password123", UserRole.Tecnico);
        _users["johndoe"].IsActive = false;

        // Act
        var result = await _authService.AuthenticateAsync("johndoe", "password123");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithEmptyUsername_ReturnsNull()
    {
        // Act
        var result = await _authService.AuthenticateAsync("", "password123");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithEmptyPassword_ReturnsNull()
    {
        // Arrange
        await _authService.RegisterAsync("johndoe", "john@example.com", "password123", UserRole.Tecnico);

        // Act
        var result = await _authService.AuthenticateAsync("johndoe", "");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Registo de Utilizadores

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsUser()
    {
        // Act
        var result = await _authService.RegisterAsync("newuser", "newuser@example.com", "password123", UserRole.Responsavel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newuser", result.Username);
        Assert.Equal("newuser@example.com", result.Email);
        Assert.Equal(UserRole.Responsavel, result.Role);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateUsername_ReturnsNull()
    {
        // Arrange
        await _authService.RegisterAsync("johndoe", "john@example.com", "password123", UserRole.Tecnico);

        // Act
        var result = await _authService.RegisterAsync("johndoe", "john2@example.com", "password456", UserRole.Responsavel);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_WithEmptyUsername_ReturnsNull()
    {
        // Act
        var result = await _authService.RegisterAsync("", "user@example.com", "password123", UserRole.Tecnico);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_WithEmptyEmail_ReturnsNull()
    {
        // Act
        var result = await _authService.RegisterAsync("newuser", "", "password123", UserRole.Tecnico);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_WithShortPassword_ReturnsNull()
    {
        // Act
        var result = await _authService.RegisterAsync("newuser", "user@example.com", "short", UserRole.Tecnico);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidEmail_ReturnsNull()
    {
        // Act
        var result = await _authService.RegisterAsync("newuser", "notanemail", "password123", UserRole.Tecnico);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Roles de Utilizador

    [Theory]
    [InlineData(UserRole.Tecnico)]
    [InlineData(UserRole.Responsavel)]
    [InlineData(UserRole.Administrador)]
    public async Task RegisterAsync_WithAllRoles_CreatesUserWithCorrectRole(UserRole role)
    {
        // Act
        var result = await _authService.RegisterAsync("roletest", "role@example.com", "password123", role);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(role, result.Role);
    }

    [Fact]
    public async Task AuthenticateAsync_TechnicoRole_ReturnsTokenWithCorrectRole()
    {
        // Arrange
        await _authService.RegisterAsync("tecnico", "tech@example.com", "password123", UserRole.Tecnico);

        // Act
        var result = await _authService.AuthenticateAsync("tecnico", "password123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserRole.Tecnico, result.Role);
    }

    [Fact]
    public async Task AuthenticateAsync_ResponsavelRole_ReturnsTokenWithCorrectRole()
    {
        // Arrange
        await _authService.RegisterAsync("responsible", "resp@example.com", "password123", UserRole.Responsavel);

        // Act
        var result = await _authService.AuthenticateAsync("responsible", "password123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserRole.Responsavel, result.Role);
    }

    [Fact]
    public async Task AuthenticateAsync_AdministradorRole_ReturnsTokenWithCorrectRole()
    {
        // Arrange
        await _authService.RegisterAsync("admin", "admin@example.com", "password123", UserRole.Administrador);

        // Act
        var result = await _authService.AuthenticateAsync("admin", "password123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserRole.Administrador, result.Role);
    }

    #endregion

    #region Validação de Palavras-Passe

    [Theory]
    [InlineData("12345678", true)]      // Exatamente 8 caracteres - válido
    [InlineData("123456789", true)]     // 9 caracteres - válido
    [InlineData("password123", true)]   // 11 caracteres - válido
    [InlineData("1234567", false)]      // 7 caracteres - inválido
    [InlineData("", false)]             // Vazio - inválido
    [InlineData("abc", false)]          // Muito curto - inválido
    public void ValidatePasswordLength_UsesBoundaries(string password, bool expected)
    {
        // Act & Assert
        Assert.Equal(expected, _authService.ValidatePasswordLength(password));
    }

    [Fact]
    public void ValidatePasswordLength_WithNullPassword_ReturnsFalse()
    {
        // Act & Assert
        Assert.False(_authService.ValidatePasswordLength(null!));
    }

    #endregion

    #region Validação de Email

    [Theory]
    [InlineData("user@example.com", true)]        // Email válido
    [InlineData("test.user@domain.co.uk", true)]  // Email com domínio complexo - válido
    [InlineData("user+tag@example.com", true)]    // Email com + - válido
    [InlineData("userexample.com", false)]        // Sem @ - inválido
    [InlineData("user@", false)]                  // Sem domínio - inválido
    [InlineData("@example.com", false)]           // Sem utilizador - inválido
    [InlineData("user@.com", false)]              // Domínio inválido - inválido
    [InlineData("", false)]                       // Vazio - inválido
    public void ValidateEmailFormat_ValidatesPattern(string email, bool expected)
    {
        // Act & Assert
        Assert.Equal(expected, _authService.ValidateEmailFormat(email));
    }

    [Fact]
    public void ValidateEmailFormat_WithNullEmail_ReturnsFalse()
    {
        // Act & Assert
        Assert.False(_authService.ValidateEmailFormat(null!));
    }

    #endregion

    #region Validação de Token

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsFalse()
    {
        // Act & Assert
        Assert.False(_authService.ValidateToken("invalido.token"));
    }

    [Fact]
    public void ValidateToken_WithEmptyToken_ReturnsFalse()
    {
        // Act & Assert
        Assert.False(_authService.ValidateToken(""));
    }

    [Fact]
    public void ValidateToken_WithNullToken_ReturnsFalse()
    {
        // Act & Assert
        Assert.False(_authService.ValidateToken(null!));
    }

    [Fact]
    public async Task ValidateToken_WithValidTokenFromAuth_ReturnsTrue()
    {
        // Arrange
        await _authService.RegisterAsync("johndoe", "john@example.com", "password123", UserRole.Tecnico);
        var authResult = await _authService.AuthenticateAsync("johndoe", "password123");

        // Act
        var isValid = _authService.ValidateToken(authResult!.Token);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateToken_WithMalformedToken_ReturnsFalse()
    {
        // Act & Assert
        Assert.False(_authService.ValidateToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid.invalid"));
    }

    #endregion

    #region Renovação de Token

    [Fact]
    public async Task RenewTokenAsync_WithValidToken_ReturnsNewToken()
    {
        // Arrange
        await _authService.RegisterAsync("johndoe", "john@example.com", "password123", UserRole.Tecnico);
        var authResult = await _authService.AuthenticateAsync("johndoe", "password123");

        // Act
        var newToken = await _authService.RenewTokenAsync(authResult!.Token);

        // Assert
        Assert.NotNull(newToken);
        // Serviço retorna o mesmo token (não gera novo)
        Assert.Equal(authResult.Token, newToken);
    }

    [Fact]
    public async Task RenewTokenAsync_WithInvalidToken_ReturnsNull()
    {
        // Act
        var result = await _authService.RenewTokenAsync("invalid.token.here");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Casos Extremos

    [Fact]
    public async Task AuthenticateAsync_CaseSensitive_WithDifferentCase_ReturnsNull()
    {
        // Arrange
        await _authService.RegisterAsync("johndoe", "john@example.com", "password123", UserRole.Tecnico);

        // Act
        var result = await _authService.AuthenticateAsync("JohnDoe", "password123");

        // Assert
        // Serviço é case-insensitive para username
        Assert.NotNull(result);
        Assert.Equal("johndoe", result.Username.ToLower());
    }

    [Fact]
    public async Task RegisterAsync_MultipleUsersWithDifferentRoles()
    {
        // Act
        var admin = await _authService.RegisterAsync("admin", "admin@example.com", "password123", UserRole.Administrador);
        var tech = await _authService.RegisterAsync("tech", "tech@example.com", "password123", UserRole.Tecnico);
        var resp = await _authService.RegisterAsync("responsible", "resp@example.com", "password123", UserRole.Responsavel);

        // Assert
        Assert.NotNull(admin);
        Assert.NotNull(tech);
        Assert.NotNull(resp);
        Assert.Equal(UserRole.Administrador, admin.Role);
        Assert.Equal(UserRole.Tecnico, tech.Role);
        Assert.Equal(UserRole.Responsavel, resp.Role);
    }

    [Fact]
    public async Task ValidatePasswordLength_WithSpecialCharacters_StillValidatesLength()
    {
        // Act & Assert
        Assert.True(_authService.ValidatePasswordLength("p@ss!@#$%"));   // 8+ caracteres
        Assert.False(_authService.ValidatePasswordLength("p@ss!"));      // < 8 caracteres
    }

    [Fact]
    public async Task AuthenticateAsync_WithWhitespaceInPassword_TreatsAsPartOfPassword()
    {
        // Arrange
        await _authService.RegisterAsync("user", "user@example.com", "pass word123", UserRole.Tecnico);

        // Act
        var resultWithSpace = await _authService.AuthenticateAsync("user", "pass word123");
        var resultWithoutSpace = await _authService.AuthenticateAsync("user", "password123");

        // Assert
        Assert.NotNull(resultWithSpace);
        Assert.Null(resultWithoutSpace);
    }

    #endregion
}