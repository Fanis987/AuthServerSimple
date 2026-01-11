using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthServerSimple.Application.Services;

/// <summary>
/// Implementation of <see cref="IJwtTokenService"/> for generating JWT tokens using configured options.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="jwtOptions">The JWT configuration options.</param>
    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    /// <inheritdoc />
    public string? GenerateToken(string userId, string userName, IEnumerable<string> roles, string requestedAudience)
    {
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

        if (audience == null) return null;
        
        // Token generation
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}