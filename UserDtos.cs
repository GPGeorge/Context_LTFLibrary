// DTOs/UserDtos.cs
using System.ComponentModel.DataAnnotations;

namespace LTF_Library_V1.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName
        {
            get; set;
        }
        public string? LastName
        {
            get; set;
        }
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedDate
        {
            get; set;
        }
        public DateTime? LastLoginDate
        {
            get; set;
        }
        public bool IsActive
        {
            get; set;
        }
        public bool EmailConfirmed
        {
            get; set;
        }
        public List<string> Roles { get; set; } = new();
    }

    public class LoginResult
    {
        public bool Success
        {
            get; set;
        }
        public string Message { get; set; } = string.Empty;
        public UserDto? User
        {
            get; set;
        }
        public List<string> Errors { get; set; } = new();
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class AssignRoleDto
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}