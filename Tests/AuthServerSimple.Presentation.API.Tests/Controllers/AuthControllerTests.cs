using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Dtos;
using AuthServerSimple.Dtos.Requests;
using AuthServerSimple.Dtos.Responses;
using AuthServerSimple.Infrastructure.Identity;
using AuthServerSimple.Presentation.API.Controllers;
using FakeItEasy;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthServerSimple.Presentation.API.Tests.Controllers;

public class AuthControllerTests
{
    // Dependencies
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRoleRepository _roleRepository;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    
    // SUT
    private readonly AuthController _controller;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthControllerTests()
    {
        _userManager = A.Fake<UserManager<ApplicationUser>>(options => options.WithArgumentsForConstructor(new object[] 
        { 
            A.Fake<IUserStore<ApplicationUser>>(), 
            null!, null!, null!, null!, null!, null!, null!, null! 
        }));
        
        _signInManager = A.Fake<SignInManager<ApplicationUser>>(options => options.WithArgumentsForConstructor(new object[]
        {
            _userManager,
            A.Fake<IHttpContextAccessor>(),
            A.Fake<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null!, null!, null!, null!
        }));
        
        _jwtTokenService = A.Fake<IJwtTokenService>();
        _registerValidator = A.Fake<IValidator<RegisterRequest>>();
        _loginValidator = A.Fake<IValidator<LoginRequest>>();
        _roleRepository = A.Fake<IRoleRepository>();

        // Setup validators to pass by default unless explicitly configured in tests
        var registerResult = new FluentValidation.Results.ValidationResult();
        A.CallTo(() => _registerValidator.ValidateAsync(A<RegisterRequest>.Ignored, A<CancellationToken>.Ignored))
            .Returns(registerResult);
            
        var loginResult = new FluentValidation.Results.ValidationResult();
        A.CallTo(() => _loginValidator.ValidateAsync(A<LoginRequest>.Ignored, A<CancellationToken>.Ignored))
            .Returns(loginResult);
        
        _controller = new AuthController(_userManager,_roleRepository, _signInManager, _jwtTokenService, _registerValidator, _loginValidator);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenRegistrationSucceeds()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "Password123!", "Admin");
        A.CallTo(() => _roleRepository.RoleExistsAsync("Admin")).Returns(true);
        A.CallTo(() => _userManager.CreateAsync(A<ApplicationUser>.Ignored, request.Password))
            .Returns(IdentityResult.Success);
        A.CallTo(() => _userManager.AddToRoleAsync(A<ApplicationUser>.Ignored, request.Role))
            .Returns(IdentityResult.Success);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RegisterResponse>(okResult.Value);
        Assert.True(response.IsSuccess);
        Assert.Equal("User registered successfully", response.Message);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "Password123!", "Admin");
        var errors = new[] { new IdentityError { Description = "Error 1" }, new IdentityError { Description = "Error 2" } };
        A.CallTo(() => _roleRepository.RoleExistsAsync("Admin")).Returns(true);
        A.CallTo(() => _userManager.CreateAsync(A<ApplicationUser>.Ignored, request.Password))
            .Returns(IdentityResult.Failed(errors));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<RegisterResponse>(badRequestResult.Value);
        Assert.False(response.IsSuccess);
        Assert.Contains("Error 1", response.Message);
        Assert.Contains("Error 2", response.Message);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenRoleAssignmentFails()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "Password123!", "Admin");
        A.CallTo(() => _roleRepository.RoleExistsAsync("Admin")).Returns(true);
        A.CallTo(() => _userManager.CreateAsync(A<ApplicationUser>.Ignored, request.Password))
            .Returns(IdentityResult.Success);
        A.CallTo(() => _userManager.AddToRoleAsync(A<ApplicationUser>.Ignored, request.Role))
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Role error" }));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<RegisterResponse>(badRequestResult.Value);
        Assert.False(response.IsSuccess);
        Assert.Equal("Role error", response.Message);
    }

    [Fact]
    public async Task Login_ReturnsOkWithToken_WhenLoginSucceeds()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Password123!", false);
        var user = new ApplicationUser { Id = "user-id", UserName = request.Email, Email = request.Email };
        var roles = new List<string> { "User" };
        var token = "generated-jwt-token";

        A.CallTo(() => _signInManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe, false))
            .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);
        A.CallTo(() => _userManager.FindByEmailAsync(request.Email))
            .Returns(user);
        A.CallTo(() => _userManager.GetRolesAsync(user))
            .Returns(roles);
        A.CallTo(() => _jwtTokenService.GenerateToken(user.Id, user.UserName, roles))
            .Returns(token);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.True(response.IsSuccess);
        Assert.Equal("Login successful", response.Message);
        Assert.Equal(token, response.Token);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenAccountLockedOut()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Password123!", false);
        A.CallTo(() => _signInManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe, false))
            .Returns(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(unauthorizedResult.Value);
        Assert.False(response.IsSuccess);
        Assert.Equal("User account locked out", response.Message);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenLoginFails()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "wrong-password", false);
        A.CallTo(() => _signInManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe, false))
            .Returns(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(unauthorizedResult.Value);
        Assert.False(response.IsSuccess);
        Assert.Equal("Invalid login attempt", response.Message);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserNotFoundAfterSuccess()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Password123!", false);
        A.CallTo(() => _signInManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe, false))
            .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);
        A.CallTo(() => _userManager.FindByEmailAsync(request.Email))
            .Returns((ApplicationUser?)null);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(unauthorizedResult.Value);
        Assert.False(response.IsSuccess);
        Assert.Equal("Invalid login attempt", response.Message);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenUserHasNoRoles()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Password123!", false);
        var user = new ApplicationUser { Id = "user-id", UserName = request.Email, Email = request.Email };
        
        A.CallTo(() => _signInManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe, false))
            .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);
        A.CallTo(() => _userManager.FindByEmailAsync(request.Email))
            .Returns(user);
        A.CallTo(() => _userManager.GetRolesAsync(user))
            .Returns(new List<string>());

        // Act
        var result = await _controller.Login(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(badRequestResult.Value);
        Assert.False(response.IsSuccess);
        Assert.Equal("User has no roles", response.Message);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var request = new RegisterRequest("", "", "");
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Email", "Email is required."),
            new("Password", "Password is required.")
        };
        A.CallTo(() => _registerValidator.ValidateAsync(request, A<CancellationToken>.Ignored))
            .Returns(new FluentValidation.Results.ValidationResult(validationFailures));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<RegisterResponse>(badRequestResult.Value);
        Assert.False(response.IsSuccess);
        Assert.Contains("Email is required.", response.Message);
        Assert.Contains("Password is required.", response.Message);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var request = new LoginRequest("", "", false);
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Email", "Email is required.")
        };
        A.CallTo(() => _loginValidator.ValidateAsync(request, A<CancellationToken>.Ignored))
            .Returns(new FluentValidation.Results.ValidationResult(validationFailures));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(badRequestResult.Value);
        Assert.False(response.IsSuccess);
        Assert.Contains("Email is required.", response.Message);
    }
}
