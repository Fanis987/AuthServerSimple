namespace AuthServerSimple.Domain;

public class AuthServerException : Exception
{
    public AuthServerException(string message)
        : base(message){}
}