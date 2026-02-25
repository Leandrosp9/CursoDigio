using CursoDigio.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CursoDigio.Models
{
    public class Enrollment
    {
        [Key]
        [Required]
        public int Id { get; set; }
       
        public int StudentId { get; set; }
        public Student? Student { get; set; }
       
        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public StatusEnrollment Status { get; set; }
        public DateTime DataMatricula { get; set; }
    }
}