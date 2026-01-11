using AuthServerSimple.Application.Validation;
using AuthServerSimple.Dtos.Requests;

namespace AuthServerSimple.Application.Tests.Validation;

public class TokenRequestValidatorTests
{
    private readonly TokenRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var request = new TokenRequest("", "Password123!", "test-audience");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TokenRequest.Email) 
                                            && e.ErrorMessage == "Email is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var request = new TokenRequest("invalid-email", "Password123!", "test-audience");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TokenRequest.Email) 
                                            && e.ErrorMessage == "A valid email address is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var request = new TokenRequest("test@example.com", "", "test-audience");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TokenRequest.Password) 
                                            && e.ErrorMessage == "Password is required.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = new TokenRequest("test@example.com", "Password123!", "test-audience");
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Audience_Is_Empty()
    {
        var request = new TokenRequest("test@example.com", "Password123!", "");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TokenRequest.Audience) 
                                            && e.ErrorMessage == "Audience is required.");
    }
}
