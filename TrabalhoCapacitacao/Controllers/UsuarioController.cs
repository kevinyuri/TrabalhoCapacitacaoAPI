using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TrabalhoCapacitacao.Data;
using TrabalhoCapacitacao.DTOs.Usuario;
using TrabalhoCapacitacao.Models;

namespace TrabalhoCapacitacao.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly IConfiguration _configuration;

        public UsuariosController(
            AppDbContext context,
            IPasswordHasher<Usuario> passwordHasher,
            IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        // POST: api/Usuarios/registrar
        [HttpPost("registrar")]
        [AllowAnonymous] // Permitir acesso anónimo para registo
        public async Task<ActionResult<UsuarioResponseDto>> RegistrarUsuario([FromBody] UsuarioCreateDto usuarioCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Usuarios.AnyAsync(u => u.Email == usuarioCreateDto.Email))
            {
                ModelState.AddModelError("Email", "Este e-mail já está em uso.");
                return BadRequest(ModelState);
            }

            var usuario = new Usuario
            {
                Nome = usuarioCreateDto.Nome,
                Email = usuarioCreateDto.Email,
                Perfil = usuarioCreateDto.Perfil, // Certifique-se de que este perfil é válido
                Telefone = usuarioCreateDto.Telefone
                // A propriedade SenhaHash será preenchida abaixo
            };

            // Gerar o hash da senha
            // O primeiro argumento para HashPassword pode ser null se não estiver a usar 'user' para particularidades do hashing
            usuario.SenhaHash = _passwordHasher.HashPassword(null, usuarioCreateDto.Senha);

            _context.Usuarios.Add(usuario);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Adicionar log mais detalhado do erro
                Console.WriteLine($"Erro ao registrar utilizador: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao registrar o utilizador no banco de dados." });
            }

            var usuarioResponseDto = new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Perfil = usuario.Perfil,
                Telefone = usuario.Telefone
            };
            // Retorna 201 Created com a localização do novo recurso e o recurso criado.
            // O utilizador precisará fazer login separadamente.
            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuarioResponseDto);
        }

        // POST: api/Usuarios/login
        //[HttpPost("login")]
        //[AllowAnonymous] // Permitir acesso anónimo para login
        //public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        //    if (usuario == null)
        //    {
        //        // Resposta genérica para não revelar se o e-mail existe ou não
        //        return Unauthorized(new { message = "Credenciais inválidas." });
        //    }

        //    // Verificar a senha
        //    // O primeiro argumento para VerifyHashedPassword pode ser null se não usou 'user' no HashPassword
        //    var resultadoPassword = _passwordHasher.VerifyHashedPassword(null, usuario.SenhaHash, loginDto.Senha);
        //    if (resultadoPassword == PasswordVerificationResult.Failed)
        //    {
        //        return Unauthorized(new { message = "Credenciais inválidas." });
        //    }
        //    // Se resultadoPassword for PasswordVerificationResult.SuccessRehashNeeded,
        //    // você pode querer fazer o re-hash da senha e atualizar no banco (para maior segurança futura).

        //    //    // Se a senha estiver correta, gerar o token JWT
        //    //    var claims = new List<Claim>
        //    //{
        //    //    new Claim(ClaimTypes.NameIdentifier, usuario.Id), // ID do utilizador (sub)
        //    //    new Claim(ClaimTypes.Email, usuario.Email),
        //    //    new Claim(ClaimTypes.Name, usuario.Nome), // Nome para exibição
        //    //    new Claim(ClaimTypes.Role, usuario.Perfil) // Adiciona o perfil como um role
        //    //    // Adicione outros claims conforme necessário (ex: roles específicos)
        //    //};

        //    var jwtSecret = _configuration["JWT:Secret"];
        //    if (string.IsNullOrEmpty(jwtSecret))
        //    {
        //        // Log de erro crítico: a chave secreta JWT não está configurada.
        //        Console.WriteLine("Erro crítico: Chave secreta JWT não configurada em appsettings.json.");
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro de configuração do servidor." });
        //    }
        //    var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        //    var credenciais = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256Signature); // Usar HmacSha256Signature

        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(claims),
        //        Expires = DateTime.UtcNow.AddHours(1), // Tempo de expiração do token (ex: 1 hora) - configure conforme necessário
        //        Audience = _configuration["JWT:ValidAudience"],
        //        Issuer = _configuration["JWT:ValidIssuer"],
        //        SigningCredentials = credenciais
        //    };

        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var token = tokenHandler.CreateToken(tokenDescriptor);

        //    return Ok(new
        //    {
        //        token = tokenHandler.WriteToken(token),
        //        expiration = token.ValidTo,
        //        usuario = new UsuarioResponseDto // Retornar também os dados do utilizador
        //        {
        //            Id = usuario.Id,
        //            Nome = usuario.Nome,
        //            Email = usuario.Email,
        //            Perfil = usuario.Perfil,
        //            Telefone = usuario.Telefone
        //        }
        //    });
        //}


        // GET: api/Usuarios/{id}
        [HttpGet("{id}")]
        //[Authorize] // Apenas utilizadores autenticados podem aceder
        public async Task<ActionResult<UsuarioResponseDto>> GetUsuario(string id)
        {
            // Lógica de autorização: permitir que o próprio utilizador ou um admin aceda
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("admin"); // Assumindo que "admin" é um perfil/role

            if (id != currentUserId && !isAdmin)
            {
                // Se não for o próprio utilizador e não for admin, proibir o acesso.
                return Forbid(); // Retorna 403 Forbidden
            }

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new { message = $"Utilizador com ID {id} não encontrado." });
            }

            var usuarioDto = new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Perfil = usuario.Perfil,
                Telefone = usuario.Telefone
            };
            return Ok(usuarioDto);
        }

        // GET: api/Usuarios
        [HttpGet]
        //[Authorize(Roles = "admin")] // Apenas utilizadores com o perfil/role "admin" podem listar todos
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new UsuarioResponseDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Perfil = u.Perfil,
                    Telefone = u.Telefone
                })
                .ToListAsync();
            return Ok(usuarios);
        }


        // PUT: api/Usuarios/{id}
        [HttpPut("{id}")]
        [Authorize] 
        public async Task<IActionResult> PutUsuario(string id, [FromBody] UsuarioUpdateDto usuarioUpdateDto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("admin");

            if (id != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuarioExistente = await _context.Usuarios.FindAsync(id);

            if (usuarioExistente == null)
            {
                return NotFound(new { message = $"Utilizador com ID {id} não encontrado para atualização." });
            }

            // Atualizar os campos permitidos
            usuarioExistente.Nome = usuarioUpdateDto.Nome;
            usuarioExistente.Telefone = usuarioUpdateDto.Telefone;

            // Apenas um admin pode alterar o perfil de outro utilizador.
            // O próprio utilizador não deve poder escalar o seu perfil.
            if (isAdmin) // Se for admin, pode mudar o perfil.
            {
                usuarioExistente.Perfil = usuarioUpdateDto.Perfil;
            }
            else if (usuarioExistente.Perfil != usuarioUpdateDto.Perfil)
            {

            }


            _context.Entry(usuarioExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"Erro de concorrência ao atualizar utilizador: {ex.ToString()}");
                if (!await UsuarioExistsAsync(id))
                {
                    return NotFound(new { message = $"Utilizador com ID {id} não encontrado (concorrência)." });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro de concorrência ao atualizar o utilizador." });
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Erro ao atualizar utilizador: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao atualizar o utilizador no banco de dados." });
            }

            return NoContent(); // Sucesso, sem conteúdo para retornar
        }

        // DELETE: api/Usuarios/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // Apenas administradores podem deletar utilizadores
        public async Task<IActionResult> DeleteUsuario(string id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound(new { message = $"Utilizador com ID {id} não encontrado para exclusão." });
            }

            // Verificar se há inscrições associadas antes de deletar (devido ao DeleteBehavior.Restrict)
            var inscricoesAssociadas = await _context.Inscricoes.AnyAsync(i => i.UsuarioId == id);
            if (inscricoesAssociadas)
            {
                return BadRequest(new { message = "Não é possível excluir o utilizador pois existem inscrições associadas a ele. Remova as inscrições primeiro." });
            }

            _context.Usuarios.Remove(usuario);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Erro ao excluir utilizador: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao excluir o utilizador no banco de dados." });
            }

            return NoContent();
        }

        private async Task<bool> UsuarioExistsAsync(string id)
        {
            return await _context.Usuarios.AnyAsync(e => e.Id == id);
        }
    }

}
