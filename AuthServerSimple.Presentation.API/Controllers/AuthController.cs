using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Dtos.Requests;
using AuthServerSimple.Dtos.Responses;
using AuthServerSimple.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

    public AuthController(
        UserManager<ApplicationUser> userManager, 
        IRoleRepository roleRepository,
        SignInManager<ApplicationUser> signInManager, 
        IJwtTokenService jwtTokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<TokenRequest> loginValidator)
    {
        _userManager = userManager;
        _roleRepository = roleRepository;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Validation
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(RegisterResponse.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }
        
        if (!await _roleRepository.RoleExistsAsync(request.Role))
            return BadRequest(RegisterResponse.Failure("Requested role does not exist"));

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
                    return BadRequest(RegisterResponse.Failure(string.Join(", ", roleResult.Errors.Select(e => e.Description))));
                }
                return Ok(RegisterResponse.Success("User registered successfully"));
            }

            return BadRequest(RegisterResponse.Failure(string.Join(", ", result.Errors.Select(e => e.Description))));
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPost("token")]
    public async Task<IActionResult> Login([FromBody] TokenRequest request)
    {
        //Validation
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(AuthResponse.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        try
        {
            var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, lockoutOnFailure: false);
            if (result.IsLockedOut)
            {
                return Unauthorized(AuthResponse.Failure("User account locked out"));
            }

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Unauthorized(AuthResponse.Failure("Invalid login attempt"));
                }

                // Check for roles in the user
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Count == 0)
                    return BadRequest(AuthResponse.Failure("User has no roles"));

                // Prepare and return the token
                var token = _jwtTokenService.GenerateToken(user.Id, user.UserName!, roles, request.Audience);
                if (token == null) return BadRequest("invalid Audience");

                return Ok(AuthResponse.Success("Login successful", token));
            }

            return Unauthorized(AuthResponse.Failure("Invalid login attempt"));
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occurred.");
        }
    }
}