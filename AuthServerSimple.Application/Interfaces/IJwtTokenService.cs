namespace AuthServerSimple.Application.Interfaces;

/// <summary>
/// Service for generating JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for a user with specified roles.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="userName">The name of the user.</param>
    /// <param name="roles">The roles assigned to the user.</param>
    /// <returns>A string representation of the generated JWT token.</returns>
    public string GenerateToken(string userId, string userName, IEnumerable<string> roles);
}