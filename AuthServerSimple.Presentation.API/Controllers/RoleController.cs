using System.ComponentModel.DataAnnotations;
using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Domain;
using AuthServerSimple.Dtos.Requests;
using AuthServerSimple.Dtos.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthServerSimple.Presentation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IValidator<CreateRoleRequest> _createRoleValidator;
    private readonly IValidator<UpdateRoleRequest> _updateRoleValidator;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        IRoleRepository roleRepository,
        IValidator<CreateRoleRequest> createRoleValidator,
        IValidator<UpdateRoleRequest> updateRoleValidator,
        ILogger<RoleController> logger)
    {
        _roleRepository = roleRepository;
        _createRoleValidator = createRoleValidator;
        _updateRoleValidator = updateRoleValidator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            _logger.LogInformation("Retrieving all roles");
            
            var roles = await _roleRepository.GetAllRolesAsync();
            var response = roles.Select(role => new RoleResponse(role.Name!));
            return Ok(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while retrieving all roles");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        _logger.LogInformation("Attempting to create role {RoleName}", request.RoleName);
        //Validation
        var validationResult = await _createRoleValidator.ValidateAsync(request);
        if (!validationResult.IsValid) {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        try
        {
            var result = await _roleRepository.CreateRoleAsync(request.RoleName);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to create role {RoleName}: {Errors}", request.RoleName, string.Join(", ", result.Errors));
                return BadRequest(result.Errors);
            }

            _logger.LogInformation("Role {RoleName} created successfully", request.RoleName);
            return Created();
        }
        catch (AuthServerException authEx)
        {
            _logger.LogError(authEx, "A domain error occurred while creating role {RoleName}", request.RoleName);
            return StatusCode(500, authEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating role {RoleName}", request.RoleName);
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
    {
        _logger.LogInformation("Attempting to update role from {OldRoleName} to {NewRoleName}", request.OldRoleName, request.NewRoleName);
        //Validation
        var validationResult = await _updateRoleValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Update role validation failed for {OldRoleName}: {Errors}", request.OldRoleName, errors);
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        try
        {
            var result = await _roleRepository.UpdateRoleAsync(request.OldRoleName, request.NewRoleName);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to update role {OldRoleName}: {Errors}", request.OldRoleName, string.Join(", ", result.Errors));
                return BadRequest(result.Errors);
            }

            _logger.LogInformation("Role {OldRoleName} updated to {NewRoleName} successfully", request.OldRoleName, request.NewRoleName);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating role {OldRoleName}", request.OldRoleName);
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpDelete("{roleName}")]
    public async Task<IActionResult> DeleteRole(string roleName)
    {
        _logger.LogInformation("Attempting to delete role {RoleName}", roleName);
        //Validation
        if(string.IsNullOrWhiteSpace(roleName)) 
        {
            _logger.LogWarning("Delete role failed: roleName is empty");
            return BadRequest(new ValidationResult("roleName cannot be empty"));
        }
        
        try
        {
            var result = await _roleRepository.DeleteRoleAsync(roleName);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to delete role {RoleName}: {Errors}", roleName, string.Join(", ", result.Errors));
                return BadRequest(result.Errors);
            }

            _logger.LogInformation("Role {RoleName} deleted successfully", roleName);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting role {RoleName}", roleName);
            return StatusCode(500, "An internal server error occurred.");
        }
    }
}