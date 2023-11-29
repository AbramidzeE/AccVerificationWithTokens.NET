using System.ComponentModel.DataAnnotations;

namespace AccVerificationWithTokens.Model
{
    public class RegisterRequest
    {
        [Required]
        public int ID { get; set; }
        public string UserName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(6, ErrorMessage = "Pls enter at least 6 character")]
        public string Password { get; set; } = string.Empty;
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
