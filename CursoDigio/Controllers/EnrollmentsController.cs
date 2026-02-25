using CursoDigio.Data;
using CursoDigio.Domain.Enums;
using CursoDigio.DTOs.Request;
using CursoDigio.DTOs.Response;
using CursoDigio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace CursoDigio.Controllers
{
    /// <summary>
    /// Gerenciamento de matrículas de estudantes em cursos.
    /// </summary>
    /// <remarks>
    /// Responsável por matricular estudantes em cursos e consultar matrículas.
    /// 
    /// Regras de segurança:
    /// - Estudante só pode matricular a si mesmo.
    /// - Admin pode matricular qualquer estudante.
    /// - StudentId nunca é confiado ao cliente quando o usuário é Student.
    /// - Evita duplicidade de matrícula.
    /// - Utiliza transação para consistência.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IdentityDbContext _context;

        public EnrollmentsController(IdentityDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Realiza matrícula de estudante em um curso.
        /// </summary>
        /// <remarks>
        /// Requer autenticação JWT.
        /// 
        /// Regras:
        /// - Usuário Student: matrícula apenas em si mesmo (via token).
        /// - Admin: pode informar StudentId no payload.
        /// - Não permite matrícula duplicada.
        /// - Curso deve existir.
        /// 
        /// Exemplo (Student logado):
        ///
        ///     POST /api/enrollments
        ///     {
        ///         "courseId": 2
        ///     }
        ///
        /// Exemplo (Admin matriculando aluno):
        ///
        ///     POST /api/enrollments
        ///     {
        ///         "courseId": 2,
        ///         "studentId": 5
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Matrícula criada com sucesso</response>
        /// <response code="400">StudentId obrigatório para Admin</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="404">Curso ou estudante não encontrado</response>
        /// <response code="409">Matrícula duplicada</response>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<StudentResponse>> CreateEnrollment(CreateEnrollmentRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            int studentId;

            // Regra de segurança CRÍTICA
            if (isAdmin)
            {
                if (request.StudentId == null)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "StudentId obrigatório",
                        Detail = "Admin deve informar StudentId.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                studentId = request.StudentId;
            }
            else
            {
                // Lookup correto no SEU modelo
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UsuarioId == userId && !s.IsDeleted);

                if (student == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Perfil não encontrado",
                        Detail = "Usuário autenticado não possui perfil de estudante.",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                studentId = student.Id;
            }

            // Validar curso
            var courseExists = await _context.Courses
                .AnyAsync(c => c.Id == request.CourseId);

            if (!courseExists)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Curso não encontrado",
                    Detail = $"Course {request.CourseId} não existe.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            //Blindagem contra duplicidade lógica
            var duplicate = await _context.Enrollments.AnyAsync(e =>
                e.CourseId == request.CourseId &&
                e.StudentId == studentId &&
                e.Status == StatusEnrollment.Ativo);

            if (duplicate)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Matrícula duplicada",
                    Detail = "O estudante já possui matrícula ativa neste curso.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var enrollment = new Enrollment
                {
                    CourseId = request.CourseId,
                    StudentId = studentId,
                    Status = StatusEnrollment.Ativo,
                    DataMatricula = DateTime.UtcNow
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return StatusCode(StatusCodes.Status201Created);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Lista matrículas de um estudante.
        /// </summary>
        /// <remarks>
        /// Requer autenticação JWT.
        /// 
        /// Regras:
        /// - Admin pode ver matrículas de qualquer estudante.
        /// - Student pode ver apenas suas próprias matrículas.
        /// - Retorna cursos ativos do estudante.
        /// 
        /// Exemplo:
        ///
        ///     GET /api/enrollments/5/enrollments
        ///
        /// </remarks>
        /// <response code="200">Lista de matrículas retornada</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="403">Sem permissão para acessar dados de outro estudante</response>
        /// <response code="404">Estudante não encontrado</response>
        [HttpGet("{id}/enrollments")]
        [Authorize]
        public async Task<ActionResult<StudentResponse>> GetStudentEnrollments(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Se não for Admin só pode acessar o próprio perfil
            if (!isAdmin)
            {
                var ownStudentId = await _context.Students
                    .Where(s => s.UsuarioId == userId && !s.IsDeleted)
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();

                if (ownStudentId == 0)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Perfil não encontrado",
                        Detail = "Usuário não possui perfil de estudante.",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                if (ownStudentId != id)
                {
                    return Forbid(); 
                }
            }

            // Validar estudante alvo
            var studentExists = await _context.Students.AnyAsync(s =>
                s.Id == id && !s.IsDeleted);

            if (!studentExists)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Estudante não encontrado",
                    Detail = $"Student {id} não existe.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            // Query correta (JOIN com Course)
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == id && e.Status == StatusEnrollment.Ativo)
                .Select(e => new
                {
                    e.Course.Id,
                    e.Course.Titulo,
                    e.Course.Categoria,
                    e.DataMatricula
                })
                .ToListAsync();

            return Ok(enrollments);
        }
    }
}
   
