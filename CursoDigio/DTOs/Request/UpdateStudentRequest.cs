using System.ComponentModel.DataAnnotations;

namespace CursoDigio.DTOs.Request
{
    public class UpdateStudentRequest
    {
        [Required]
        [StringLength(120)]
        public string NomeCompleto { get; set; } = string.Empty;
    }
}
