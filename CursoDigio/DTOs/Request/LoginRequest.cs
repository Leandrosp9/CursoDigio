using System.ComponentModel.DataAnnotations;

namespace CursoDigio.DTOs.Request
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
