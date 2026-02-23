using CursoDigio.Data;
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
    /// Gerenciamento de perfis de estudantes (vinculados ao ASP.NET Core Identity).
    /// </summary>
    /// <remarks>
    /// Este controller gerencia o recurso "Student" (perfil acadêmico), que é vinculado a um usuário do Identity via UsuarioId.
    /// 
    /// Regras gerais:
    /// - Apenas Admin pode listar todos os estudantes.
    /// - Admin pode criar perfil de estudante para um UserId existente no Identity.
    /// - Um estudante autenticado pode consultar/atualizar apenas o próprio perfil (via comparação do token).
    /// - Soft delete é aplicado em Student (IsDeleted = true).
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IdentityDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public StudentsController(IdentityDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Cria um perfil de estudante vinculado a um usuário do Identity.
        /// </summary>
        /// <remarks>
        /// Requer autenticação JWT.
        /// Role exigida:
        /// - Admin
        /// 
        /// Regras:
        /// - O UsuarioId informado deve existir na tabela AspNetUsers (Identity).
        /// - Não permite criar perfil duplicado para o mesmo UsuarioId (retorna 409).
        /// 
        /// Exemplo de requisição:
        /// 
        ///     POST /api/students
        ///     {
        ///         "nomeCompleto": "João da Silva",
        ///         "usuarioId": "d10eada4-f46f-4c22-b1e0-b4dcbc7ebe58"
        ///     }
        /// </remarks>
        /// <response code="201">Perfil criado com sucesso</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="403">Usuário sem permissão (não Admin)</response>
        /// <response code="404">Usuário do Identity não encontrado</response>
        /// <response code="409">Já existe perfil de estudante para o UsuarioId informado</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StudentResponse>> CreateStudent(CreateStudentRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UsuarioId);

            if (user == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Usuário não encontrado",
                    Detail = $"UserId {request.UsuarioId} não existe no Identity.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            //Evita duplicidade de perfil
            var exists = await _context.Students
                .AnyAsync(s => s.UsuarioId == request.UsuarioId);

            if (exists)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Perfil já existe",
                    Detail = "Este usuário já possui perfil de estudante.",
                    Status = StatusCodes.Status409Conflict,
                    Instance = HttpContext.Request.Path
                });
            }

            var student = new Student
            {
                NomeCompleto = request.NomeCompleto,
                UsuarioId = request.UsuarioId,
                DataCadastro = DateTime.UtcNow
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var response = new StudentResponse
            {
                Id = student.Id,
                NomeCompleto = student.NomeCompleto,
                DataCadastro = student.DataCadastro,
                UserId = student.UsuarioId,
                Email = user.Email
            };

            return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, response);
        }

        /// <summary>
        /// Obtém um estudante pelo ID.
        /// </summary>
        /// <remarks>
        /// Requer autenticação JWT.
        /// 
        /// Regra de acesso (recomendado para privacidade):
        /// - Admin pode consultar qualquer estudante
        /// - Estudante comum deve consultar apenas o próprio perfil
        /// 
        /// Observação:
        /// O endpoint retorna o email a partir do Identity (AspNetUsers).
        /// </remarks>
        /// <response code="200">Estudante encontrado</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="403">Usuário sem permissão (não Admin e não dono do perfil)</response>
        /// <response code="404">Estudante não encontrado</response>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<StudentResponse>> GetStudentById (int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(student.UsuarioId);

            var response = new StudentResponse
            {
                Id = student.Id,
                NomeCompleto = student.NomeCompleto,
                DataCadastro = student.DataCadastro,
                UserId = student.UsuarioId,
                Email = user?.Email
            };

            return Ok(response);
        }

        /// <summary>
        /// Lista todos os estudantes (dados sensíveis).
        /// </summary>
        /// <remarks>
        /// Requer autenticação JWT.
        /// Role exigida:
        /// - Admin
        /// </remarks>
        /// <response code="200">Lista de estudantes retornada com sucesso</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="403">Usuário sem permissão (não Admin)</response>

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StudentResponse>> GetStudents()
        {
            var students = await _context.Students.ToListAsync();
            var response = new List<StudentResponse>();

            foreach (var student in students)
            {
                var user = await _userManager.FindByIdAsync(student.UsuarioId);
                response.Add(new StudentResponse
                {
                    Id = student.Id,
                    NomeCompleto = student.NomeCompleto,
                    DataCadastro = student.DataCadastro,
                    UserId = student.UsuarioId,
                    Email = user?.Email
                });
            }
            return Ok(response);
        }

        /// <summary>
        /// Atualiza o perfil de estudante.
        /// </summary>
        /// <remarks>
        /// Requer autenticação JWT.
        /// 
        /// Regras de acesso:
        /// - Admin pode atualizar qualquer estudante
        /// - Estudante comum pode atualizar apenas o próprio perfil (comparando UsuarioId com o "sub" do token)
        /// 
        /// Exemplo:
        /// 
        ///     PUT /api/students/10
        ///     {
        ///         "nomeCompleto": "Novo Nome"
        ///     }
        /// </remarks>
        /// <response code="200">Perfil atualizado com sucesso</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="403">Usuário sem permissão</response>
        /// <response code="404">Estudante não encontrado</response>
        [HttpPut]
        [Authorize]
        public async Task<ActionResult<StudentResponse>> UpdateStudent(int id, UpdateStudentRequest request)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
               return NotFound();

            var loggedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var isOwner = student.UsuarioId == loggedUserId;

            if (!isAdmin && !isOwner)
                return Forbid();

            student.NomeCompleto = request.NomeCompleto;

            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(student.UsuarioId);

            return Ok(new StudentResponse
            {
                Id = student.Id,
                NomeCompleto = student.NomeCompleto,
                DataCadastro = student.DataCadastro,
                UserId = student.UsuarioId,
                Email = user?.Email
            });
        }

        /// <summary>
        /// Atualiza o perfil de estudante.
        /// </summary>
        /// <remarks>
        /// Requer autenticação JWT.
        /// 
        /// Regras de acesso:
        /// - Admin pode atualizar qualquer estudante
        /// - Estudante comum pode atualizar apenas o próprio perfil (comparando UsuarioId com o "sub" do token)
        /// 
        /// Exemplo:
        /// 
        ///     PUT /api/students/10
        ///     {
        ///         "nomeCompleto": "Novo Nome"
        ///     }
        /// </remarks>
        /// <response code="200">Perfil atualizado com sucesso</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="403">Usuário sem permissão</response>
        /// <response code="404">Estudante não encontrado</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StudentResponse>> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null || student.IsDeleted) return NotFound();

            student.IsDeleted = true;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Retorna o perfil do usuário autenticado (me).
        /// </summary>
        /// <remarks>
        /// Requer autenticação JWT.
        /// 
        /// O Student é localizado pelo UsuarioId (sub) presente no token.
        /// Este endpoint evita que o cliente precise conhecer o StudentId.
        /// </remarks>
        /// <response code="200">Perfil retornado com sucesso</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="404">Perfil do estudante não encontrado</response>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<StudentResponse>> GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UsuarioId == userId && !s.IsDeleted);

            if (student == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(userId);

            return Ok(new StudentResponse
            {
                Id = student.Id,
                NomeCompleto = student.NomeCompleto,
                DataCadastro = student.DataCadastro,
                UserId = student.UsuarioId,
                Email = user?.Email
            });
        }

    }
}
