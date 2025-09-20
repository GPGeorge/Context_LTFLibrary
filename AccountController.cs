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
    [ApiController]
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
            // ADD THESE DEBUG LINES AT THE START
            Console.WriteLine("=== JSON LOGIN METHOD CALLED ===");
            Console.WriteLine($"=== Request URL: {HttpContext.Request.Path} ===");
            Console.WriteLine($"=== Request Method: {HttpContext.Request.Method} ===");

            try
            {
                Console.WriteLine($"Controller: Received JSON login request for {loginDto?.Username}");

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

                Console.WriteLine($"Controller: JSON SignIn result = {result.Succeeded}");

                if (result.Succeeded)
                {
                    user.LastLoginDate = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    // ADD THE SAME COOKIE DEBUG HERE
                    Console.WriteLine("=== COOKIE DEBUG START ===");
                    Console.WriteLine($"Request.Cookies.Count: {HttpContext.Request.Cookies.Count}");
                    foreach (var cookie in HttpContext.Request.Cookies)
                    {
                        Console.WriteLine($"Request Cookie: {cookie.Key} = {cookie.Value}");
                    }

                    Console.WriteLine($"Response.Headers.Count: {HttpContext.Response.Headers.Count}");
                    if (HttpContext.Response.Headers.ContainsKey("Set-Cookie"))
                    {
                        var setCookies = HttpContext.Response.Headers["Set-Cookie"];
                        Console.WriteLine($"Set-Cookie headers: {setCookies.Count}");
                        foreach (var setCookie in setCookies)
                        {
                            Console.WriteLine($"Set-Cookie: {setCookie}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("NO Set-Cookie headers found!");
                    }

                    Console.WriteLine($"User.Identity.IsAuthenticated: {HttpContext.User.Identity.IsAuthenticated}");
                    Console.WriteLine($"User.Identity.Name: {HttpContext.User.Identity.Name}");
                    Console.WriteLine($"PathBase: '{HttpContext.Request.PathBase}'");
                    Console.WriteLine($"Path: '{HttpContext.Request.Path}'");
                    Console.WriteLine($"Host: '{HttpContext.Request.Host}'");
                    Console.WriteLine("=== COOKIE DEBUG END ===");

                    Console.WriteLine("Controller: JSON login successful");

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
                Console.WriteLine($"Controller: JSON Login Exception = {ex.Message}");
                Console.WriteLine($"Controller: JSON Login Stack Trace = {ex.StackTrace}");
                _logger.LogError(ex, "Login error for user {Username}", loginDto.Username);
                return BadRequest(new
                {
                    success = false,
                    message = "An error occurred during login"
                });
            }
        }
        // Add this method to your AccountController class

        [HttpGet("test")]
        public IActionResult Test()
        {
            Console.WriteLine("=== ACCOUNT CONTROLLER TEST ENDPOINT CALLED ===");
            return Ok(new
            {
                message = "AccountController is working!",
                path = HttpContext.Request.Path,
                pathBase = HttpContext.Request.PathBase,
                host = HttpContext.Request.Host.ToString()
            });
        }
      
        [HttpPost("loginform")]
        public async Task<IActionResult> LoginForm([FromForm] LoginDto loginDto)
        {
            try
            {
                // Calculate paths based on environment
                var isLocal = HttpContext.Request.Host.Host.Contains("localhost");
                var adminPath = isLocal ? "/admin" : "/LTFCatalog/admin";
                var loginPath = isLocal ? "/login" : "/LTFCatalog/login";

                Console.WriteLine($"Controller: Received form login request for {loginDto?.Username}");
                Console.WriteLine($"Controller: isLocal = {isLocal}");
                Console.WriteLine($"Controller: adminPath = {adminPath}");
                Console.WriteLine($"Controller: loginPath = {loginPath}");

                if (loginDto == null || string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
                {
                    Console.WriteLine("Controller: Invalid form data");
                    return Redirect($"{loginPath}?error=Username and password are required");
                }

                var user = await _userManager.FindByNameAsync(loginDto.Username);
                Console.WriteLine($"Controller: User found = {user != null}");

                if (user == null || !user.IsActive)
                {
                    Console.WriteLine("Controller: User not found or inactive");
                    return Redirect($"{loginPath}?error=Invalid username or password");
                }

                var result = await _signInManager.PasswordSignInAsync(
                    loginDto.Username,
                    loginDto.Password,
                    loginDto.RememberMe,
                    lockoutOnFailure: false);

                Console.WriteLine($"Controller: SignIn result = {result.Succeeded}");

                if (result.Succeeded)
                {
                    user.LastLoginDate = DateTime.Now;
                    await _userManager.UpdateAsync(user);
                    Console.WriteLine($"Controller: Form login successful, redirecting to {adminPath}");
                    Console.WriteLine("=== COOKIE DEBUG START ===");
                    Console.WriteLine($"Request.Cookies.Count: {HttpContext.Request.Cookies.Count}");
                    foreach (var cookie in HttpContext.Request.Cookies)
                    {
                        Console.WriteLine($"Request Cookie: {cookie.Key} = {cookie.Value}");
                    }

                    Console.WriteLine($"Response.Headers.Count: {HttpContext.Response.Headers.Count}");
                    if (HttpContext.Response.Headers.ContainsKey("Set-Cookie"))
                    {
                        var setCookies = HttpContext.Response.Headers["Set-Cookie"];
                        Console.WriteLine($"Set-Cookie headers: {setCookies.Count}");
                        foreach (var setCookie in setCookies)
                        {
                            Console.WriteLine($"Set-Cookie: {setCookie}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("NO Set-Cookie headers found!");
                    }

                    Console.WriteLine($"User.Identity.IsAuthenticated: {HttpContext.User.Identity.IsAuthenticated}");
                    Console.WriteLine($"User.Identity.Name: {HttpContext.User.Identity.Name}");
                    Console.WriteLine($"PathBase: '{HttpContext.Request.PathBase}'");
                    Console.WriteLine($"Path: '{HttpContext.Request.Path}'");
                    Console.WriteLine($"Host: '{HttpContext.Request.Host}'");
                    Console.WriteLine("=== COOKIE DEBUG END ===");

                    Console.WriteLine($"Controller: Form login successful, redirecting to {adminPath}");
                    return Redirect(adminPath);
                }

                Console.WriteLine("Controller: SignIn failed");
                return Redirect($"{loginPath}?error=Invalid username or password");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Controller: LoginForm Exception = {ex.Message}");
                Console.WriteLine($"Controller: LoginForm Stack Trace = {ex.StackTrace}");
                _logger.LogError(ex, "Form login error for user {Username}", loginDto.Username);

                var isLocal = HttpContext.Request.Host.Host.Contains("localhost");
                var loginPath = isLocal ? "/login" : "/LTFCatalog/login";

                TempData["ErrorMessage"] = "An error occurred during login";
                return Redirect(loginPath);
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