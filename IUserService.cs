// Services/IUserService.cs
using LTF_Library_V1.DTOs;
using System.Security.Claims;

namespace LTF_Library_V1.Services
{
    public interface IUserService
    {
        Task<LoginResult> LoginAsync(LoginDto loginDto);
        Task<LoginResult> RegisterAsync(RegisterDto registerDto);
        Task<bool> LogoutAsync();

        Task<UserDto?> GetCurrentUserAsync();
        Task<UserDto?> GetCurrentUserByClaimsPrincipalAsync(ClaimsPrincipal? claimsPrincipal);

        Task<UserDto?> GetUserByIdAsync(string userId);
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<List<UserDto>> GetAllUsersAsync();

        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
        Task<bool> AssignRoleAsync(string userId, string roleName);
        Task<bool> RemoveFromRoleAsync(string userId, string roleName);

        Task<List<string>> GetUserRolesAsync(string userId);
        Task<bool> IsInRoleAsync(string userId, string roleName);

        Task<bool> UpdateUserAsync(string userId, UserDto userDto);
        Task<bool> DeactivateUserAsync(string userId);
        Task<bool> ActivateUserAsync(string userId);
    }
}
