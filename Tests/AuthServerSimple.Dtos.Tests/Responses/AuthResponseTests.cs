using AuthServerSimple.Dtos.Responses;

namespace AuthServerSimple.Dtos.Tests.Responses;

public class AuthResponseTests
{
    [Fact]
    public void Success_ShouldReturnSuccessfulAuthResponse()
    {
        // Arrange
        var message = "Success message";
        var token = "some-token";

        // Act
        var response = AuthResponse.Success(message, token);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(message, response.Message);
        Assert.Equal(token, response.Token);
    }

    [Fact]
    public void Success_WithDefaultMessage_ShouldReturnSuccessfulAuthResponse()
    {
        // Act
        var response = AuthResponse.Success();

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal("Success", response.Message);
        Assert.Null(response.Token);
    }

    [Fact]
    public void Failure_ShouldReturnFailedAuthResponse()
    {
        // Arrange
        var message = "Error message";

        // Act
        var response = AuthResponse.Failure(message);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal(message, response.Message);
        Assert.Null(response.Token);
    }
}
