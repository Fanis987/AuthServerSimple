namespace AuthServerSimple.Dtos;

public record LoginRequest(string Email, string Password, bool RememberMe, string Audience);