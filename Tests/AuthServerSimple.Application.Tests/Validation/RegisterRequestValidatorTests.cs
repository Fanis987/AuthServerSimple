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

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("test@com")]
    [InlineData("test@.com")]
    [InlineData("@example.com")]
    [InlineData("test@example")]
    [InlineData("test@example.")]
    public void Should_Have_Error_When_Email_Is_Invalid(string email)
    {
        var request = new RegisterRequest(email, "Password123!", "Admin");
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
                                            && e.ErrorMessage == "Password must be at least 6 characters long.");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Missing_Capital_Letter()
    {
        var request = new RegisterRequest("test@example.com", "password123!", "Admin");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Password) 
                                            && e.ErrorMessage == "Password must contain at least one capital letter.");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Only_Numerical()
    {
        var request = new RegisterRequest("test@example.com", "1234567", "Admin");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Password) 
                                            && e.ErrorMessage == "Password must contain at least one non-numerical character.");
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

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("user45.name@domain.com")]
    [InlineData("user.name@domain.co.sth.uk")]
    [InlineData("user+tag@example.com")]
    public void Should_Not_Have_Error_When_Email_Is_Valid(string email)
    {
        var request = new RegisterRequest(email, "Password123!", "Admin");
        var result = _validator.Validate(request);
        Assert.True(result.IsValid, string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = new RegisterRequest("test@example.com", "Password123!", "Admin");
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }
}
