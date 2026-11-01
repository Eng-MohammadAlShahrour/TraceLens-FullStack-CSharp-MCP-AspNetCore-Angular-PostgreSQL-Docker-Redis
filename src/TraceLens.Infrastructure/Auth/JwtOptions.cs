namespace TraceLens.Infrastructure.Auth;

public sealed class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
    public DemoUserOptions DemoUser { get; set; } = new();
}

public sealed class DemoUserOptions
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
    public string DisplayName { get; set; } = string.Empty;
}
