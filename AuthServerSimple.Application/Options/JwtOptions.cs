namespace AuthServerSimple.Application.Options;

public class JwtOptions
{
    public const string JwtOptionsSectionName = "JwtOptions";
    
    public string IssuerSigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; }
}