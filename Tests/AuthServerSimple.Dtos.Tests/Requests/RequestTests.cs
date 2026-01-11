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

        // Act
        var request = new TokenRequest(email, password, audience);

        // Assert
        Assert.Equal(email, request.Email);
        Assert.Equal(password, request.Password);
        Assert.Equal(audience, request.Audience);
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
