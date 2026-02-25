using System.ComponentModel.DataAnnotations;

namespace CursoDigio.DTOs.Request
{
    public class CreateStudentRequest
    {
        [Required]
        public string? UsuarioId { get; set; }
        [Required]
        [StringLength(120)]
        public string? NomeCompleto { get; set; }
    }
}
