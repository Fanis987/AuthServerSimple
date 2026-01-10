using AuthServerSimple.Application.Validation;
using AuthServerSimple.Dtos.Requests;
using FluentValidation;

namespace AuthServerSimple.Application.Tests.Validation;

public class UpdateRoleRequestValidatorTests
{
    private readonly UpdateRoleRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_OldRoleName_Is_Empty()
    {
        var request = new UpdateRoleRequest("", "NewRole");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRoleRequest.OldRoleName) 
                                            && e.ErrorMessage == "Old role name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_NewRoleName_Is_Empty()
    {
        var request = new UpdateRoleRequest("OldRole", "");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRoleRequest.NewRoleName) 
                                            && e.ErrorMessage == "New role name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_NewRoleName_Exceeds_100_Characters()
    {
        var request = new UpdateRoleRequest("OldRole", new string('a', 101));
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRoleRequest.NewRoleName) 
                                            && e.ErrorMessage == "New role name must not exceed 100 characters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = new UpdateRoleRequest("OldRole", "NewRole");
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }
}
