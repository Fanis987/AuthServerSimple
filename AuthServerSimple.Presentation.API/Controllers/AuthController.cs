using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Dtos;
using AuthServerSimple.Dtos.Requests;
using AuthServerSimple.Dtos.Responses;
using AuthServerSimple.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthServerSimple.Presentation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    //Dependencies
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe, lockoutOnFailure: false);
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
                var token = _jwtTokenService.GenerateToken(user.Id, user.UserName!, roles);

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