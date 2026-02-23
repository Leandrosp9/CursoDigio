using System.ComponentModel.DataAnnotations;

namespace CursoDigio.Models
{
    public class Course
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required(ErrorMessage = "O Titulo é obrigatório.")]
        public string? Titulo { get; set; }
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        public string? Descricao { get; set; }
        [Required(ErrorMessage = "A categoria é obrigatória.")]
        public string? Categoria { get; set; }
        [Required(ErrorMessage = "A carga é obrigatória.")]
        public int  CargaHoraria { get; set; }
        public DateTime DataCriacao { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }

        public Course()
        {
            Enrollments = new List<Enrollment>();
        }
    }
}
