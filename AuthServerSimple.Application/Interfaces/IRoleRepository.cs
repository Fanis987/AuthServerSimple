using Microsoft.AspNetCore.Identity;

namespace AuthServerSimple.Application.Interfaces;

/// <summary>
/// Repository interface for managing identity roles.
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Gets all roles from the identity store.
    /// </summary>
    /// <returns>A collection of identity roles.</returns>
    Task<IEnumerable<IdentityRole>> GetAllRolesAsync();

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="roleName">The name of the role to create.</param>
    /// <returns>The result of the operation.</returns>
    Task<IdentityResult> CreateRoleAsync(string roleName);

    /// <summary>
    /// Updates the name of an existing role.
    /// </summary>
    /// <param name="oldRoleName">The current name of the role.</param>
    /// <param name="newRoleName">The new name for the role.</param>
    /// <returns>The result of the operation.</returns>
    Task<IdentityResult> UpdateRoleAsync(string oldRoleName, string newRoleName);

    /// <summary>
    /// Deletes a role.
    /// </summary>
    /// <param name="roleName">The name of the role to delete.</param>
    /// <returns>The result of the operation.</returns>
    Task<IdentityResult> DeleteRoleAsync(string roleName);
}
