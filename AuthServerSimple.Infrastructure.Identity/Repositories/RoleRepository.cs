using AuthServerSimple.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthServerSimple.Infrastructure.Identity.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleRepository(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IdentityRole>> GetAllRolesAsync()
    {
        return await _roleManager.Roles.ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<IdentityResult> CreateRoleAsync(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName)) {
            return IdentityResult.Failed(new IdentityError { Description = $"Role {roleName} already exists." });
        }

        return await _roleManager.CreateAsync(new IdentityRole(roleName));
    }

    /// <inheritdoc/>
    public async Task<IdentityResult> UpdateRoleAsync(string oldRoleName, string newRoleName)
    {
        var role = await _roleManager.FindByNameAsync(oldRoleName);
        if (role == null) {
            return IdentityResult.Failed(new IdentityError { Description = $"Role {oldRoleName} not found." });
        }

        role.Name = newRoleName;
        return await _roleManager.UpdateAsync(role);
    }

    /// <inheritdoc/>
    public async Task<IdentityResult> DeleteRoleAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) {
            return IdentityResult.Failed(new IdentityError { Description = $"Role {roleName} not found." });
        }

        return await _roleManager.DeleteAsync(role);
    }
}
