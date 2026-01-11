using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Domain;
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
        try
        {
            var roles = await _roleRepository.GetAllRolesAsync();
            var response = roles.Select(role => new RoleResponse(role.Name!));
            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        try
        {
            var result = await _roleRepository.CreateRoleAsync(request.RoleName);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Created();
        }
        catch (AuthServerException authEx)
        {
            return StatusCode(500, authEx.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
    {
        try
        {
            var result = await _roleRepository.UpdateRoleAsync(request.OldRoleName, request.NewRoleName);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpDelete("{roleName}")]
    public async Task<IActionResult> DeleteRole(string roleName)
    {
        try
        {
            var result = await _roleRepository.DeleteRoleAsync(roleName);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occurred.");
        }
    }
}