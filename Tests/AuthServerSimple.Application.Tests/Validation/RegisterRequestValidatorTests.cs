using AuthServerSimple.Application.Validation;
using AuthServerSimple.Dtos.Requests;

namespace AuthServerSimple.Application.Tests.Validation;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var request = new RegisterRequest("", "Password123!", "Admin");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Email) 
                                            && e.ErrorMessage == "Email is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var request = new RegisterRequest("invalid-email", "Password123!", "Admin");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Email) 
                                            && e.ErrorMessage == "A valid email address is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var request = new RegisterRequest("test@example.com", "", "Admin");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Password) 
                                            && e.ErrorMessage == "Password is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        var request = new RegisterRequest("test@example.com", "Pass1", "Admin");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Password) 
                                            && e.ErrorMessage == "Password must be at least 6 characters long and contain a capital and a non-alphanumeric character.");
    }

    [Fact]
    public void Should_Have_Error_When_Role_Is_Empty()
    {
        var request = new RegisterRequest("test@example.com", "Password123!", "");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Role) 
                                            && e.ErrorMessage == "Role is required.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = new RegisterRequest("test@example.com", "Password123!", "Admin");
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }
}
