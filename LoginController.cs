// Controllers/LoginController.cs
using Microsoft.AspNetCore.Mvc;
using LTF_Library_V1.Services;
using LTF_Library_V1.DTOs;

namespace LTF_Library_V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IUserService userService, ILogger<LoginController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("=== CONTROLLER LOGIN ATTEMPT for {Username} ===", loginDto.Username);

                var result = await _userService.LoginAsync(loginDto);

                _logger.LogInformation("Login result: Success={Success}, Message={Message}", result.Success, result.Message);

                if (result.Success)
                {
                    // Determine redirect URL based on environment
                    var isLocal = Request.Host.Value.Contains("localhost");
                    var adminUrl = isLocal ? "/admin" : "/LTFCatalog/admin" ;

                    _logger.LogInformation("Successful login, should redirect to: {AdminUrl}", adminUrl);

                    return Ok(new
                    {
                        success = true,
                        message = "Login successful",
                        redirectUrl = adminUrl
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = result.Message,
                        errors = result.Errors
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", loginDto.Username);
                return Ok(new
                {
                    success = false,
                    message = "An error occurred during login"
                });
            }
        }
    }
}