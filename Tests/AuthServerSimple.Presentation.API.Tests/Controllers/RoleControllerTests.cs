using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Dtos.Requests;
using AuthServerSimple.Dtos.Responses;
using AuthServerSimple.Presentation.API.Controllers;
using FakeItEasy;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthServerSimple.Presentation.API.Tests.Controllers;

public class RoleControllerTests
{
    private readonly IRoleRepository _roleRepository;
    private readonly RoleController _controller;
    private readonly IValidator<CreateRoleRequest> _createRoleValidator;
    private readonly IValidator<UpdateRoleRequest> _updateRoleValidator;

    public RoleControllerTests()
    {
        _roleRepository = A.Fake<IRoleRepository>();
        _createRoleValidator = A.Fake<IValidator<CreateRoleRequest>>();
        _updateRoleValidator = A.Fake<IValidator<UpdateRoleRequest>>();

        // Setup validators to pass by default
        A.CallTo(() => _createRoleValidator.ValidateAsync(A<CreateRoleRequest>.Ignored, A<CancellationToken>.Ignored))
            .Returns(new FluentValidation.Results.ValidationResult());
        A.CallTo(() => _updateRoleValidator.ValidateAsync(A<UpdateRoleRequest>.Ignored, A<CancellationToken>.Ignored))
            .Returns(new FluentValidation.Results.ValidationResult());

        _controller = new RoleController(_roleRepository, _createRoleValidator, _updateRoleValidator);
    }

    [Fact]
    public async Task GetAllRoles_ReturnsOk_WithRoles()
    {
        // Arrange
        var roles = new List<IdentityRole>
        {
            new IdentityRole("Admin"),
            new IdentityRole("User")
        };
        A.CallTo(() => _roleRepository.GetAllRolesAsync()).Returns(roles);

        // Act
        var result = await _controller.GetAllRoles();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<RoleResponse>>(okResult.Value);
        Assert.Equal(2, response.Count());
    }

    [Fact]
    public async Task CreateRole_ReturnsCreated_WhenSucceeds()
    {
        // Arrange
        var request = new CreateRoleRequest("Manager");
        A.CallTo(() => _roleRepository.CreateRoleAsync(request.RoleName))
            .Returns(IdentityResult.Success);

        // Act
        var result = await _controller.CreateRole(request);

        // Assert
        Assert.IsType<CreatedResult>(result);
    }

    [Fact]
    public async Task CreateRole_ReturnsBadRequest_WhenFails()
    {
        // Arrange
        var request = new CreateRoleRequest("Manager");
        var errors = new[] { new IdentityError { Description = "Error" } };
        A.CallTo(() => _roleRepository.CreateRoleAsync(request.RoleName))
            .Returns(IdentityResult.Failed(errors));

        // Act
        var result = await _controller.CreateRole(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errors, badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateRole_ReturnsNoContent_WhenSucceeds()
    {
        // Arrange
        var request = new UpdateRoleRequest("OldName", "NewName");
        A.CallTo(() => _roleRepository.UpdateRoleAsync(request.OldRoleName, request.NewRoleName))
            .Returns(IdentityResult.Success);

        // Act
        var result = await _controller.UpdateRole(request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateRole_ReturnsBadRequest_WhenFails()
    {
        // Arrange
        var request = new UpdateRoleRequest("OldName", "NewName");
        var errors = new[] { new IdentityError { Description = "Error" } };
        A.CallTo(() => _roleRepository.UpdateRoleAsync(request.OldRoleName, request.NewRoleName))
            .Returns(IdentityResult.Failed(errors));

        // Act
        var result = await _controller.UpdateRole(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errors, badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteRole_ReturnsNoContent_WhenSucceeds()
    {
        // Arrange
        var roleName = "Manager";
        A.CallTo(() => _roleRepository.DeleteRoleAsync(roleName))
            .Returns(IdentityResult.Success);

        // Act
        var result = await _controller.DeleteRole(roleName);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteRole_ReturnsBadRequest_WhenFails()
    {
        // Arrange
        var roleName = "Manager";
        var errors = new[] { new IdentityError { Description = "Error" } };
        A.CallTo(() => _roleRepository.DeleteRoleAsync(roleName))
            .Returns(IdentityResult.Failed(errors));

        // Act
        var result = await _controller.DeleteRole(roleName);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errors, badRequestResult.Value);
    }

    [Fact]
    public async Task GetAllRoles_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        A.CallTo(() => _roleRepository.GetAllRolesAsync()).Throws(new Exception());

        // Act
        var result = await _controller.GetAllRoles();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("An internal server error occurred.", objectResult.Value);
    }

    [Fact]
    public async Task CreateRole_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var request = new CreateRoleRequest("");
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("RoleName", "Role name is required.")
        };
        A.CallTo(() => _createRoleValidator.ValidateAsync(request, A<CancellationToken>.Ignored))
            .Returns(new FluentValidation.Results.ValidationResult(validationFailures));

        // Act
        var result = await _controller.CreateRole(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(badRequestResult.Value);
        Assert.Contains("Role name is required.", errors);
    }

    [Fact]
    public async Task UpdateRole_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var request = new UpdateRoleRequest("", "");
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("NewRoleName", "New role name is required.")
        };
        A.CallTo(() => _updateRoleValidator.ValidateAsync(request, A<CancellationToken>.Ignored))
            .Returns(new FluentValidation.Results.ValidationResult(validationFailures));

        // Act
        var result = await _controller.UpdateRole(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(badRequestResult.Value);
        Assert.Contains("New role name is required.", errors);
    }
}
