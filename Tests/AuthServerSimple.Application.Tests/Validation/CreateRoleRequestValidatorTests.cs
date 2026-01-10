using AuthServerSimple.Application.Validation;
using AuthServerSimple.Dtos.Requests;
using FluentValidation;

namespace AuthServerSimple.Application.Tests.Validation;

public class CreateRoleRequestValidatorTests
{
    private readonly CreateRoleRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_RoleName_Is_Empty()
    {
        var request = new CreateRoleRequest("");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRoleRequest.RoleName) 
                                            && e.ErrorMessage == "Role name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_RoleName_Exceeds_100_Characters()
    {
        var request = new CreateRoleRequest(new string('a', 101));
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRoleRequest.RoleName) 
                                            && e.ErrorMessage == "Role name must not exceed 100 characters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_RoleName_Is_Valid()
    {
        var request = new CreateRoleRequest("Admin");
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }
}
