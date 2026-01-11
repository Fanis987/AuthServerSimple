using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthServerSimple.Application.Services;

/// <summary>
/// Implementation of <see cref="IJwtTokenService"/> for generating JWT tokens using configured options.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<JwtTokenService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="jwtOptions">The JWT configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public JwtTokenService(IOptions<JwtOptions> jwtOptions, ILogger<JwtTokenService> logger)
    {
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public string? GenerateToken(string userId, string userName, 
        IEnumerable<string> roles, string requestedAudience, int? durationInMinutes = null)
    {
        _logger.LogInformation("Generating token for user {UserName} ({UserId}) with requested audience {RequestedAudience}", userName, userId, requestedAudience);
        // Choosing claims
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.UniqueName, userName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.IssuerSigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Choosing Audience 
        string? audience = null;
        foreach (var existingAudience in _jwtOptions.Audiences) {
            if (requestedAudience == existingAudience)
            {
                audience = existingAudience;
                break;
            }
        }

        if (audience == null)
        {
            _logger.LogWarning("Token generation failed: requested audience {RequestedAudience} is not valid.", requestedAudience);
            return null;
        }
        
        // Choosing Duration
        var durationInMin = durationInMinutes ?? _jwtOptions.ExpiresInMinutes;
        
        // Token generation
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(durationInMin),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        _logger.LogInformation("Token generated successfully for user {UserName}.", userName);
        return tokenString;
    }
}