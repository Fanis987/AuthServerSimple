namespace AuthServerSimple.Dtos.Responses;

public record RegisterResponse(bool IsSuccess, string Message)
{
    public static RegisterResponse Success(string message = "Success") 
        => new(true, message);

    public static RegisterResponse Failure(string message) 
        => new(false, message);
}
