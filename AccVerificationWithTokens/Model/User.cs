using System.ComponentModel.DataAnnotations;

namespace AccVerificationWithTokens.Model
{
    public class User
    {
        
        public int Id { get; set; }
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = new byte[32];
        public byte[] PasswordSalt { get; set; } = new byte[32];
        public string? VerificationToken { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime ResetTokenExpires { get; set; }
    }
}
