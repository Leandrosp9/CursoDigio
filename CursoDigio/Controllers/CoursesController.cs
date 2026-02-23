using CursoDigio.Data;
using CursoDigio.DTOs.Request;
using CursoDigio.DTOs.Response;
using CursoDigio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CursoDigio.Controllers
{
    /// <summary>
    /// Gerenciamento de cursos da plataforma.
    /// </summary>
    /// <remarks>
    /// Endpoints responsáveis pelo cadastro, consulta, atualização e remoção de cursos.
    /// Alguns endpoints requerem autenticação via JWT.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly IdentityDbContext _context;

        public CoursesController(IdentityDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Cria um novo curso.
        /// </summary>
        /// <remarks>
        /// Requer autenticação JWT.
        /// Roles permitidos:
        /// - Admin
        /// - Instructor
        ///
        /// Exemplo de requisição:
        ///
        ///     POST /api/courses
        ///     {
        ///         "titulo": "C# Avançado",
        ///         "descricao": "Curso completo de C#",
        ///         "categoria": "Programação",
        ///         "cargaHoraria": 40
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Curso criado com sucesso</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="403">Usuário sem permissão</response>
        /// <response code="422">Dados inválidos</response>
        [HttpPost]
        [Authorize(Roles ="Admin,Instructor")]
        public async Task<ActionResult<CourseResponse>> CreateCourse(CreateCourseRequest request)
        {
            if (request.Titulo == null || request.Titulo.Length < 3)
            {
                return UnprocessableEntity(new ProblemDetails
                {
                    Title = "Título inválido",
                    Detail = "Título deve possuir pelo menos 3 caracteres.",
                    Status = StatusCodes.Status422UnprocessableEntity
                });
            }

            var course = new Course
            {
                Titulo = request.Titulo,
                Descricao = request.Descricao,
                Categoria = request.Categoria,
                CargaHoraria = request.CargaHoraria,
                DataCriacao = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var response = new CourseResponse
            {
                Id = course.Id,
                Titulo = course.Titulo,
                Descricao = course.Descricao,
                Categoria = course.Categoria,
                CargaHoraria = course.CargaHoraria,
                DataCriacao = course.DataCriacao
            };

            return CreatedAtAction(
                nameof(GetCourseById),
                new { id = course.Id },
                response
            );
        }

        /// <summary>
        /// Obtém um curso pelo ID.
        /// </summary>
        /// <remarks>
        /// Retorna os dados completos de um curso específico.
        /// </remarks>
        /// <response code="200">Curso encontrado</response>
        /// <response code="404">Curso não encontrado</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseResponse>> GetCourseById(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Curso não encontrado",
                    Detail = $"Curso com id {id} não existe.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            var response = new CourseResponse
            {
                Id = course.Id,
                Titulo = course.Titulo,
                Descricao = course.Descricao,
                Categoria = course.Categoria,
                CargaHoraria = course.CargaHoraria,
                DataCriacao = course.DataCriacao
            };

            return Ok(response);
        }


        /// <summary>
        /// Lista cursos com paginação e filtro opcional.
        /// </summary>
        /// <remarks>
        /// Permite filtrar por categoria e controlar paginação.
        ///
        /// Exemplos:
        ///
        ///     GET /api/courses
        ///     GET /api/courses?categoria=Programação
        ///     GET /api/courses?page=2&pageSize=5
        ///
        /// </remarks>
        /// <response code="200">Lista de cursos retornada com sucesso</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetCourses(
        [FromQuery] string? categoria,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(c => c.Categoria == categoria);
            }

            var courses = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = courses.Select(course => new CourseResponse
            {
                Id = course.Id,
                Titulo = course.Titulo,
                Descricao = course.Descricao,
                Categoria = course.Categoria,
                CargaHoraria = course.CargaHoraria,
                DataCriacao = course.DataCriacao
            });

            return Ok(response);
        }


        /// <summary>
        /// Atualiza um curso existente.
        /// </summary>
        /// <remarks>
        /// Requer autenticação JWT.
        /// Roles permitidos:
        /// - Admin
        /// - Instructor
        /// </remarks>
        /// <response code="200">Curso atualizado com sucesso</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="403">Usuário sem permissão</response>
        /// <response code="404">Curso não encontrado</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult<CourseResponse>> UpdateCourse(int id, UpdateCourseRequest request)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Curso não encontrado",
                    Detail = $"Não é possível atualizar. Curso {id} não existe.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            course.Titulo = request.Titulo;
            course.Descricao = request.Descricao;
            course.Categoria = request.Categoria;
            course.CargaHoraria = request.CargaHoraria;
            
            await _context.SaveChangesAsync();

            var response = new CourseResponse
            {
                Id = course.Id,
                Titulo = course.Titulo,
                Descricao = course.Descricao,
                Categoria = course.Categoria,
                CargaHoraria = course.CargaHoraria,
                DataCriacao = course.DataCriacao
            };

            return Ok(response);
        }

        /// <summary>
        /// Remove um curso.
        /// </summary>
        /// <remarks>
        /// Requer autenticação JWT.
        /// Roles permitidos:
        /// - Admin
        /// </remarks>
        /// <response code="204">Curso removido com sucesso</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="403">Usuário sem permissão</response>
        /// <response code="404">Curso não encontrado</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Curso não encontrado",
                    Detail = $"Não é possível atualizar. Curso {id} não existe.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();

        }
    }
}
