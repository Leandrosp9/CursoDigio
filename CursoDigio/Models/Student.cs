using CursoDigio.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CursoDigio.Models
{
    public class Student
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required(ErrorMessage = "O campo nome é obrigatório.")]
        public string? NomeCompleto { get; set; }
        public DateTime DataCadastro { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
        public bool IsDeleted { get; set; }

        //FK Identity
        public string? UsuarioId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public Student()
        {
            Enrollments = new List<Enrollment>();
        }
    }
}
