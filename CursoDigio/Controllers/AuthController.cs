using CursoDigio.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CursoDigio.Controllers
{
    /// <summary>
    /// Autenticação e geração de JWT.
    /// </summary>
    /// <remarks>
    /// Responsável por autenticar usuários e gerar tokens JWT.
    /// 
    /// Fluxo:
    /// 1. Usuário envia email e senha.
    /// 2. Sistema valida no ASP.NET Core Identity.
    /// 3. Token JWT é gerado contendo:
    ///    - UserId
    ///    - Email
    ///    - Roles
    /// 4. Token deve ser usado no header Authorization:
    /// 
    ///     Authorization: Bearer {token}
    /// 
    /// Esse token permite acesso aos endpoints protegidos.
    /// </remarks>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Realiza autenticação e retorna token JWT.
        /// </summary>
        /// <remarks>
        /// Endpoint responsável por autenticar o usuário e gerar um token JWT.
        /// 
        /// Exemplo de requisição:
        ///
        ///     POST /api/auth/login
        ///     {
        ///         "email": "admin@admin.com",
        ///         "password": "Admin@123"
        ///     }
        ///
        /// Resposta:
        ///
        ///     {
        ///         "accessToken": "eyJhbGciOiJIUzI1NiIs..."
        ///     }
        ///
        /// Como usar o token:
        ///
        /// 1. Copie o accessToken.
        /// 2. No Swagger clique em "Authorize".
        /// 3. Cole apenas o token.
        /// 4. Agora é possível acessar rotas protegidas.
        /// </remarks>
        /// <response code="200">Login realizado com sucesso</response>
        /// <response code="401">Credenciais inválidas</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Credenciais inválidas",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

    }
}
