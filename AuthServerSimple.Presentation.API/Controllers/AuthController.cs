using AuthServerSimple.Dtos;
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

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded) {
            return Ok(new AuthResponse(true, "User registered successfully"));
        }

        return BadRequest(new AuthResponse(false, string.Join(", ", result.Errors.Select(e => e.Description))));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded) {
            return Ok(new AuthResponse(true, "Login successful"));
        }

        if (result.IsLockedOut) {
            return Unauthorized(new AuthResponse(false, "User account locked out"));
        }

        return Unauthorized(new AuthResponse(false, "Invalid login attempt"));
    }
    
}