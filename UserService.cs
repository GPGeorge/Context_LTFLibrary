// Services/UserService.cs
using LTF_Library_V1.Data.Models;
using LTF_Library_V1.DTOs;
using LTF_Library_V1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTF_Library_V1.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<LoginResult> LoginAsync(LoginDto loginDto)
        {
            try
            {
                Console.WriteLine($"=== LOGIN ATTEMPT: {loginDto.Username} ===");
                var user = await _userManager.FindByNameAsync(loginDto.Username);
                Console.WriteLine($"User found: {user != null}");
                if (user == null)
                {
                    Console.WriteLine("User not found");
                    return new LoginResult
                    {
                        
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }
                Console.WriteLine($"User IsActive: {user.IsActive}, EmailConfirmed: {user.EmailConfirmed}");

                if (!user.IsActive)
                {
                    Console.WriteLine("User inactive");
                    return new LoginResult
                    {
                        Success = false,
                        Message = "Account is deactivated"
                    };
                }
                Console.WriteLine("Attempting password sign in...");
                var result = await _signInManager.PasswordSignInAsync(
                    loginDto.Username,
                    loginDto.Password,
                    loginDto.RememberMe,
                    lockoutOnFailure: false);
                Console.WriteLine($"SignIn result: {result.Succeeded}, Locked: {result.IsLockedOut}, RequiresTwoFactor: {result.RequiresTwoFactor}");
                if (result.Succeeded)
                {
                    user.LastLoginDate = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    var userDto = await MapToUserDto(user);
                    return new LoginResult
                    {
                        Success = true,
                        Message = "Login successful",
                        User = userDto
                    };
                }

                return new LoginResult
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== LOGIN EXCEPTION: {ex.Message} ===");
                Console.WriteLine($"=== INNER EXCEPTION: {ex.InnerException?.Message} ===");
                Console.WriteLine($"=== STACK TRACE: {ex.StackTrace} ===");
                _logger.LogError(ex, "Error during login for user {Username}", loginDto.Username);
                return new LoginResult
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<LoginResult> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {
                    // Assign default role (you might want to make this configurable)
                    await _userManager.AddToRoleAsync(user, "Public");

                    var userDto = await MapToUserDto(user);
                    return new LoginResult
                    {
                        Success = true,
                        Message = "Registration successful",
                        User = userDto
                    };
                }

                return new LoginResult
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Username}", registerDto.Username);
                return new LoginResult
                {
                    Success = false,
                    Message = "An error occurred during registration"
                };
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return false;
            }
        }

        public async Task<UserDto?> GetCurrentUserAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
                return user != null ? await MapToUserDto(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return null;
            }
        }
                    

        public async Task<UserDto?> GetCurrentUserByClaimsPrincipalAsync(ClaimsPrincipal? claimsPrincipal)
        {
            try
            {
                if (claimsPrincipal?.Identity?.IsAuthenticated == true)
                {
                    var user = await _userManager.GetUserAsync(claimsPrincipal);
                    return user != null ? await MapToUserDto(user) : null;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user by claims principal");
                return null;
            }
        }
        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                return user != null ? await MapToUserDto(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID {UserId}", userId);
                return null;
            }
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
                return user != null ? await MapToUserDto(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username {Username}", username);
                return null;
            }
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    userDtos.Add(await MapToUserDto(user));
                }

                return userDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return new List<UserDto>();
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var result = await _userManager.ChangePasswordAsync(
                    user,
                    changePasswordDto.CurrentPassword,
                    changePasswordDto.NewPassword);

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> AssignRoleAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var result = await _userManager.AddToRoleAsync(user, roleName);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", roleName, userId);
                return false;
            }
        }

        public async Task<bool> RemoveFromRoleAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", roleName, userId);
                return false;
            }
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return new List<string>();

                return ( await _userManager.GetRolesAsync(user) ).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
                return new List<string>();
            }
        }

        public async Task<bool> IsInRoleAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                return await _userManager.IsInRoleAsync(user, roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role {RoleName} for user {UserId}", roleName, userId);
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(string userId, UserDto userDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                user.FirstName = userDto.FirstName;
                user.LastName = userDto.LastName;
                user.Email = userDto.Email;
                user.IsActive = userDto.IsActive;

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                user.IsActive = false;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ActivateUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                user.IsActive = true;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user {UserId}", userId);
                return false;
            }
        }

        private async Task<UserDto> MapToUserDto(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            };
        }
    }
}