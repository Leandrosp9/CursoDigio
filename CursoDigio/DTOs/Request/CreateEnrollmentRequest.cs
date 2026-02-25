using System.ComponentModel.DataAnnotations;

namespace CursoDigio.DTOs.Request
{
    public class CreateEnrollmentRequest
    {
        [Required]
        public int CourseId { get; set; }
        public int StudentId { get; set; }
    }
}
