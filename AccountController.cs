using LTF_Library_V1.Data.Models;
using LTF_Library_V1.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace LTF_Library_V1.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }
       
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {

                Console.WriteLine($"Controller: Received login request for {loginDto?.Username}");
                if (loginDto == null)
                {
                    Console.WriteLine("Controller: LoginDto is null");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid request data"
                    });
                }

                if (string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
                {
                    Console.WriteLine("Controller: Username or password is empty");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Username and password are required"
                    });
                }

                var user = await _userManager.FindByNameAsync(loginDto.Username);
                Console.WriteLine($"Controller: User found = {user != null}");

                if (user == null || !user.IsActive)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid username or password"
                    });
                }

                var result = await _signInManager.PasswordSignInAsync(
                    loginDto.Username,
                    loginDto.Password,
                    loginDto.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    user.LastLoginDate = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    return Ok(new
                    {
                        success = true,
                        message = "Login successful"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Invalid username or password"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Controller: Exception = {ex.Message}");
                _logger.LogError(ex, "Login error for user {Username}", loginDto.Username);
                return BadRequest(new
                {
                    success = false,
                    message = "An error occurred during login"
                });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new
            {
                success = true
            });
        }
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "User is not authenticated"
                });
            }

            return Ok(new
            {
                success = true,
                username = User.Identity.Name,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }
    }
}