using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Dtos.Requests;
using AuthServerSimple.Dtos.Responses;
using AuthServerSimple.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthServerSimple.Presentation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    //Dependencies
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRoleRepository _roleRepository;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<TokenRequest> _loginValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager, 
        IRoleRepository roleRepository,
        SignInManager<ApplicationUser> signInManager, 
        IJwtTokenService jwtTokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<TokenRequest> loginValidator,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _roleRepository = roleRepository;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for user {Email}", request.Email);
        // Validation
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid) {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Registration validation failed for user {Email}: {Errors}", request.Email, errors);
            return BadRequest(RegisterResponse.Failure(errors));
        }
        
        if (!await _roleRepository.RoleExistsAsync(request.Role))
        {
            _logger.LogWarning("Registration failed for user {Email}: Requested role {Role} does not exist", request.Email, request.Role);
            return BadRequest(RegisterResponse.Failure("Requested role does not exist"));
        }

        // User Creation
        try
        {
            var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to add role {Role} to user {Email}: {Errors}", request.Role, request.Email, roleErrors);
                    return BadRequest(RegisterResponse.Failure(roleErrors));
                }
                _logger.LogInformation("User {Email} registered successfully with role {Role}", request.Email, request.Role);
                return Ok(RegisterResponse.Success("User registered successfully"));
            }

            var creationErrors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("User creation failed for {Email}: {Errors}", request.Email, creationErrors);
            return BadRequest(RegisterResponse.Failure(creationErrors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during registration for user {Email}", request.Email);
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPost("token")]
    public async Task<IActionResult> Login([FromBody] TokenRequest request)
    {
        _logger.LogInformation("Login attempt for user {Email}", request.Email);
        //Validation
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Login validation failed for user {Email}: {Errors}", request.Email, errors);
            return BadRequest(AuthResponse.Failure(errors));
        }

        try
        {
            var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, lockoutOnFailure: false);
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Login failed for user {Email}: User account locked out", request.Email);
                return Unauthorized(AuthResponse.Failure("User account locked out"));
            }

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null) {
                    _logger.LogError("Login succeeded for user {Email} but user not found in database", request.Email);
                    return Unauthorized(AuthResponse.Failure("Invalid login attempt"));
                }

                // Check for roles in the user
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Count == 0)
                {
                    _logger.LogWarning("Login failed for user {Email}: User has no roles", request.Email);
                    return BadRequest(AuthResponse.Failure("User has no roles"));
                }

                // Prepare and return the token
                var token = _jwtTokenService.GenerateToken(
                    user.Id, user.UserName!, roles, request.Audience,request.DurationInMinutes);
                if (token == null)
                {
                    _logger.LogWarning("Login failed for user {Email}: Invalid Audience {Audience}", request.Email, request.Audience);
                    return BadRequest("invalid Audience");
                }

                _logger.LogInformation("User {Email} logged in successfully", request.Email);
                return Ok(AuthResponse.Success("Login successful", token));
            }

            _logger.LogWarning("Login failed for user {Email}: Invalid login attempt", request.Email);
            return Unauthorized(AuthResponse.Failure("Invalid login attempt"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login for user {Email}", request.Email);
            return StatusCode(500, "An internal server error occurred.");
        }
    }
}