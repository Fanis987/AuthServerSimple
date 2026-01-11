using AuthServerSimple.Application.Validation;
using AuthServerSimple.Dtos;
using FluentValidation;

namespace AuthServerSimple.Application.Tests.Validation;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var request = new LoginRequest("", "Password123!", false, "test-audience");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Email) 
                                            && e.ErrorMessage == "Email is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var request = new LoginRequest("invalid-email", "Password123!", false, "test-audience");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Email) 
                                            && e.ErrorMessage == "A valid email address is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var request = new LoginRequest("test@example.com", "", false, "test-audience");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Password) 
                                            && e.ErrorMessage == "Password is required.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = new LoginRequest("test@example.com", "Password123!", false, "test-audience");
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Audience_Is_Empty()
    {
        var request = new LoginRequest("test@example.com", "Password123!", false, "");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Audience) 
                                            && e.ErrorMessage == "Audience is required.");
    }
}
