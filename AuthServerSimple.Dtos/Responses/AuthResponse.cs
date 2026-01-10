namespace AuthServerSimple.Dtos.Responses;

public record AuthResponse(bool IsSuccess, string Message, string? Token = null)
{
    public static AuthResponse Success(string message = "Success", string? token = null) 
        => new(true, message, token);

    public static AuthResponse Failure(string message) 
        => new(false, message);
}