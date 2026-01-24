using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Wachtwoorden komen niet overeen")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
