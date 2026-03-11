using System.ComponentModel.DataAnnotations;

namespace AstralDiaryApi.Models.DTOs.Users
{
    public class LoginRequest
    {
        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, MinLength(3), MaxLength(32)]
        public required string Name { get; set; }
    }

    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
    }
}
