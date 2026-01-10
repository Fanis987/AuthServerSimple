namespace AuthServerSimple.Dtos.Requests;

public record RegisterRequest(string Email, string Password, string Role);