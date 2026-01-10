using AuthServerSimple.Dtos.Responses;

namespace AuthServerSimple.Application.Interfaces;

public interface IUserRepository
{
    public Task<IEnumerable<UserResponse>> GetAllUsersAsync();
}
