using CursoDigio.Models;
using System.ComponentModel.DataAnnotations;

namespace CursoDigio.DTOs.Request
{
    public class CreateCourseRequest
    {
        [Required]
        [StringLength(60)]
        public string? Titulo { get; set; }
        [Required]
        [StringLength(200)]
        public string? Descricao { get; set; }
        [Required]
        [StringLength(60)]
        public string? Categoria { get; set; }
        [Required]
        [Range(1, 1000)]
        public int CargaHoraria { get; set; }

    }
}
