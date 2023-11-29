using System.ComponentModel.DataAnnotations;

namespace AccVerificationWithTokens.Model
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        [Required, MinLength(6, ErrorMessage = "Pls enter at least 6 character")]
        public string Password { get; set; } = string.Empty;
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
