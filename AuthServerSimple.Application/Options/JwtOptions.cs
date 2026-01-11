namespace AuthServerSimple.Application.Options;

public class JwtOptions
{
    public const string JwtOptionsSectionName = "JwtOptions";
    
    public string IssuerSigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public List<string> Audiences { get; set; } = [];
    public int ExpiresInMinutes { get; set; }
}