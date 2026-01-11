using AuthServerSimple.Dtos.Requests;

namespace AuthServerSimple.Dtos.Tests.Requests;

public class RequestTests
{
    [Fact]
    public void TokenRequest_ShouldStoreValuesCorrectly()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var audience = "test-audience";
        var duration = 60;

        // Act
        var request = new TokenRequest(email, password, audience, duration);

        // Assert
        Assert.Equal(email, request.Email);
        Assert.Equal(password, request.Password);
        Assert.Equal(audience, request.Audience);
        Assert.Equal(duration, request.DurationInMinutes);
    }

    [Fact]
    public void RegisterRequest_ShouldStoreValuesCorrectly()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var role = "Admin";

        // Act
        var request = new RegisterRequest(email, password, role);

        // Assert
        Assert.Equal(email, request.Email);
        Assert.Equal(password, request.Password);
        Assert.Equal(role, request.Role);
    }
}
