namespace AuthServerSimple.Application.Options;

public record JwtOptions(
    string IssuerSigningKey,
    string Issuer,
    string Audience,
    int ExpiresInMinutes)
{ public const string JwtOptionsSectionName = "JwtOptions"; }