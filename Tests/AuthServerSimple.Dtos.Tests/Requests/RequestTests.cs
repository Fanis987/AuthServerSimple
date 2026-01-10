namespace AuthServerSimple.Dtos.Tests.Requests;

public class RequestTests
{
    [Fact]
    public void LoginRequest_ShouldStoreValuesCorrectly()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var rememberMe = true;

        // Act
        var request = new LoginRequest(email, password, rememberMe);

        // Assert
        Assert.Equal(email, request.Email);
        Assert.Equal(password, request.Password);
        Assert.Equal(rememberMe, request.RememberMe);
    }

    [Fact]
    public void RegisterRequest_ShouldStoreValuesCorrectly()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";

        // Act
        var request = new RegisterRequest(email, password);

        // Assert
        Assert.Equal(email, request.Email);
        Assert.Equal(password, request.Password);
    }
}
