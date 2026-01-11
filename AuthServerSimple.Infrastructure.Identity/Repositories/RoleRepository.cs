using System.Security.Authentication;
using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Domain;
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

    public async Task<bool> RoleExistsAsync(string role)
        => await _roleManager.RoleExistsAsync(role); 

    /// <inheritdoc/>
    public async Task<IEnumerable<IdentityRole>> GetAllRolesAsync()
    {
        return await _roleManager.Roles.ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<IdentityResult> CreateRoleAsync(string roleName)
    {
        // Check for the role name
        var existingRole = await _roleManager.FindByNameAsync(roleName);
        if (existingRole != null) {
            throw new AuthServerException("The role already exists");
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
