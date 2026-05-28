namespace GREENHERB.src.Models;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
