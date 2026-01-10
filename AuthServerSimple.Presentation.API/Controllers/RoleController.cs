using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Dtos.Requests;
using AuthServerSimple.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AuthServerSimple.Presentation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;

    public RoleController(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleRepository.GetAllRolesAsync();
        var response = roles.Select(role => new RoleResponse(role.Name!));
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var result = await _roleRepository.CreateRoleAsync(request.RoleName);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Created();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
    {
        var result = await _roleRepository.UpdateRoleAsync(request.OldRoleName, request.NewRoleName);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpDelete("{roleName}")]
    public async Task<IActionResult> DeleteRole(string roleName)
    {
        var result = await _roleRepository.DeleteRoleAsync(roleName);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }
}