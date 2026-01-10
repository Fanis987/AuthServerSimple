namespace AuthServerSimple.Dtos.Responses;

public record UserResponse(string Email, IEnumerable<string> Roles);
