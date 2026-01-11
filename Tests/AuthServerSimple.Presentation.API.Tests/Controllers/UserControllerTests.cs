using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Dtos.Responses;
using AuthServerSimple.Presentation.API.Controllers;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthServerSimple.Presentation.API.Tests.Controllers;

public class UserControllerTests
{
    private readonly IUserRepository _userRepository;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userRepository = A.Fake<IUserRepository>();
        var logger = A.Fake<ILogger<UserController>>();
        _controller = new UserController(_userRepository, logger);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsOk_WithUsers()
    {
        // Arrange
        var users = new List<UserResponse>
        {
            new UserResponse("user1@example.com", new List<string> { "User" }),
            new UserResponse("user2@example.com", new List<string> { "Admin" })
        };
        A.CallTo(() => _userRepository.GetAllUsersAsync()).Returns(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<UserResponse>>(okResult.Value);
        Assert.Equal(2, response.Count());
    }

    [Fact]
    public async Task GetAllUsers_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        A.CallTo(() => _userRepository.GetAllUsersAsync()).Throws(new Exception());

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("An internal server error occurred.", objectResult.Value);
    }
}