namespace AuthServerSimple.Dtos.Requests;

public record TokenRequest(string Email, string Password, string Audience);