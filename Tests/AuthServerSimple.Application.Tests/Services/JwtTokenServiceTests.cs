using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthServerSimple.Application.Options;
using AuthServerSimple.Application.Services;
using FakeItEasy;
using Microsoft.Extensions.Options;

namespace AuthServerSimple.Application.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly IOptions<JwtOptions> _jwtOptionsMock;
    private readonly JwtOptions _jwtOptions;
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        _jwtOptions = new JwtOptions()
        {
            IssuerSigningKey= "super_secret_key_that_is_at_least_32_characters_long",
            Issuer= "test_issuer",
            Audiences= ["test_audience"],
            ExpiresInMinutes= 60
        };

        _jwtOptionsMock = A.Fake<IOptions<JwtOptions>>();
        A.CallTo(() => _jwtOptionsMock.Value).Returns(_jwtOptions);

        _sut = new JwtTokenService(_jwtOptionsMock);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var userId = "123";
        var userName = "testuser";
        var roles = new[] { "Admin", "User" };

        // Act
        var token = _sut.GenerateToken(userId, userName, roles, _jwtOptions.Audiences[0]);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        Assert.Equal(_jwtOptions.Issuer, jsonToken.Issuer);
        Assert.Contains(_jwtOptions.Audiences[0], jsonToken.Audiences);
        
        var claims = jsonToken.Claims.ToList();
        Assert.Equal(userId, claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(userName, claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value);
        
        var roleClaims = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        Assert.Equal(roles.Length, roleClaims.Count);
        foreach (var role in roles)
        {
            Assert.Contains(role, roleClaims);
        }
    }

    [Fact]
    public void GenerateToken_ShouldHaveExpirationSetCorrectly()
    {
        // Arrange
        var userId = "123";
        var userName = "testuser";
        var roles = Enumerable.Empty<string>();

        // Act
        var token = _sut.GenerateToken(userId, userName, roles, _jwtOptions.Audiences[0]);

        // Assert
        Assert.NotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        var expectedExpiration = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresInMinutes);
        // Allow for some delay in execution
        Assert.True(jsonToken.ValidTo > DateTime.UtcNow);
        Assert.True(jsonToken.ValidTo <= expectedExpiration.AddSeconds(5));
    }

    [Fact]
    public void GenerateToken_ShouldReturnNull_WhenAudienceIsInvalid()
    {
        // Arrange
        var userId = "123";
        var userName = "testuser";
        var roles = Enumerable.Empty<string>();
        var invalidAudience = "invalid_audience";

        // Act
        var token = _sut.GenerateToken(userId, userName, roles, invalidAudience);

        // Assert
        Assert.Null(token);
    }
}
