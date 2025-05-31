using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoCapacitacao.Data;
using TrabalhoCapacitacao.DTOs.Inscricao;
using TrabalhoCapacitacao.Models;

namespace TrabalhoCapacitacao.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Proteger todo o controller, ou endpoints específicos
    public class InscricoesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InscricoesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Inscricoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InscricaoResponseDto>>> GetInscricoes(
            [FromQuery] string? usuarioId,
            [FromQuery] string? vagaId,
            [FromQuery] string? cursoId)
        {
            var query = _context.Inscricoes.AsQueryable();

            if (!string.IsNullOrEmpty(usuarioId))
            {
                query = query.Where(i => i.UsuarioId == usuarioId);
            }
            if (!string.IsNullOrEmpty(vagaId))
            {
                query = query.Where(i => i.VagaId == vagaId);
            }
            if (!string.IsNullOrEmpty(cursoId))
            {
                query = query.Where(i => i.CursoId == cursoId);
            }

            var inscricoes = await query
                .Select(i => new InscricaoResponseDto
                {
                    Id = i.Id,
                    DataInscricao = i.DataInscricao,
                    Status = i.Status,
                    UsuarioId = i.UsuarioId,
                    NomeUsuario = i.Usuario != null ? i.Usuario.Nome : null,
                    VagaId = i.VagaId,
                    TituloVaga = i.Vaga != null ? i.Vaga.Titulo : null,
                    CursoId = i.CursoId,
                    NomeCurso = i.Curso != null ? i.Curso.Nome : null
                })
                .ToListAsync();

            return Ok(inscricoes);
        }

        // GET: api/Inscricoes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<InscricaoResponseDto>> GetInscricao(string id)
        {
            var inscricao = await _context.Inscricoes
                .Include(i => i.Usuario) 
                .Include(i => i.Vaga)
                .Include(i => i.Curso)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inscricao == null)
            {
                return NotFound(new { message = $"Inscrição com ID {id} não encontrada." });
            }

            var inscricaoDto = new InscricaoResponseDto
            {
                Id = inscricao.Id,
                DataInscricao = inscricao.DataInscricao,
                Status = inscricao.Status,
                UsuarioId = inscricao.UsuarioId,
                NomeUsuario = inscricao.Usuario?.Nome,
                VagaId = inscricao.VagaId,
                TituloVaga = inscricao.Vaga?.Titulo,
                CursoId = inscricao.CursoId,
                NomeCurso = inscricao.Curso?.Nome
            };

            return Ok(inscricaoDto);
        }

        // POST: api/Inscricoes
        [HttpPost]
        // [Authorize] // Apenas usuários logados podem se inscrever
        public async Task<ActionResult<InscricaoResponseDto>> PostInscricao([FromBody] InscricaoCreateDto inscricaoCreateDto)
        {
            // Validação customizada para VagaId vs CursoId
            if (string.IsNullOrEmpty(inscricaoCreateDto.VagaId) && string.IsNullOrEmpty(inscricaoCreateDto.CursoId))
            {
                ModelState.AddModelError("", "É necessário fornecer um VagaId ou um CursoId para a inscrição.");
            }
            if (!string.IsNullOrEmpty(inscricaoCreateDto.VagaId) && !string.IsNullOrEmpty(inscricaoCreateDto.CursoId))
            {
                ModelState.AddModelError("", "A inscrição deve ser para uma Vaga OU um Curso, não ambos.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar se o usuário existe
            var usuario = await _context.Usuarios.FindAsync(inscricaoCreateDto.UsuarioId);
            if (usuario == null)
            {
                return BadRequest(new { message = $"Usuário com ID {inscricaoCreateDto.UsuarioId} não encontrado." });
            }

            Vaga vaga = null;
            if (!string.IsNullOrEmpty(inscricaoCreateDto.VagaId))
            {
                vaga = await _context.Vagas.FindAsync(inscricaoCreateDto.VagaId);
                if (vaga == null)
                {
                    return BadRequest(new { message = $"Vaga com ID {inscricaoCreateDto.VagaId} não encontrada." });
                }
                // Verificar duplicidade de inscrição para vaga
                if (await _context.Inscricoes.AnyAsync(i => i.UsuarioId == inscricaoCreateDto.UsuarioId && i.VagaId == inscricaoCreateDto.VagaId))
                {
                    return BadRequest(new { message = "Usuário já inscrito nesta vaga." });
                }
            }

            Curso curso = null;
            if (!string.IsNullOrEmpty(inscricaoCreateDto.CursoId))
            {
                curso = await _context.Cursos.FindAsync(inscricaoCreateDto.CursoId);
                if (curso == null)
                {
                    return BadRequest(new { message = $"Curso com ID {inscricaoCreateDto.CursoId} não encontrado." });
                }
                // Verificar duplicidade de inscrição para curso
                if (await _context.Inscricoes.AnyAsync(i => i.UsuarioId == inscricaoCreateDto.UsuarioId && i.CursoId == inscricaoCreateDto.CursoId))
                {
                    return BadRequest(new { message = "Usuário já inscrito neste curso." });
                }
            }

            var inscricao = new Inscricao
            {
                UsuarioId = inscricaoCreateDto.UsuarioId,
                VagaId = inscricaoCreateDto.VagaId,
                CursoId = inscricaoCreateDto.CursoId,
                Status = inscricaoCreateDto.Status,
                DataInscricao = DateTime.UtcNow
            };

            _context.Inscricoes.Add(inscricao);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Erro ao salvar inscrição: {ex.ToString()}");
                // Verificar se o erro é de chave duplicada (improvável com GUID, mas bom ter)
                if (await _context.Inscricoes.AnyAsync(e => e.Id == inscricao.Id))
                {
                    return Conflict(new { message = "Erro ao salvar a inscrição devido a um conflito de ID." });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao salvar a inscrição no banco de dados." });
            }

            var inscricaoResponseDto = new InscricaoResponseDto
            {
                Id = inscricao.Id,
                DataInscricao = inscricao.DataInscricao,
                Status = inscricao.Status,
                UsuarioId = inscricao.UsuarioId,
                NomeUsuario = usuario.Nome,
                VagaId = inscricao.VagaId,
                TituloVaga = vaga?.Titulo,
                CursoId = inscricao.CursoId,
                NomeCurso = curso?.Nome
            };

            return CreatedAtAction(nameof(GetInscricao), new { id = inscricao.Id }, inscricaoResponseDto);
        }

        // PUT: api/Inscricoes/{id} (Ex: para atualizar o status da inscrição)
        [HttpPut("{id}")]
        // [Authorize(Roles = "admin,empresa")] // Apenas admin ou empresa podem mudar status
        public async Task<IActionResult> PutInscricao(string id, [FromBody] InscricaoUpdateDto inscricaoUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var inscricaoExistente = await _context.Inscricoes.FindAsync(id);

            if (inscricaoExistente == null)
            {
                return NotFound(new { message = $"Inscrição com ID {id} não encontrada para atualização." });
            }

            // Apenas o Status pode ser atualizado através deste DTO
            inscricaoExistente.Status = inscricaoUpdateDto.Status;

            _context.Entry(inscricaoExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"Erro de concorrência ao atualizar inscrição: {ex.ToString()}");
                if (!await InscricaoExistsAsync(id))
                {
                    return NotFound(new { message = $"Inscrição com ID {id} não encontrada (concorrência)." });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro de concorrência ao atualizar a inscrição." });
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Erro ao atualizar inscrição: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao atualizar a inscrição no banco de dados." });
            }

            return NoContent();
        }

        // DELETE: api/Inscricoes/{id}
        [HttpDelete("{id}")]
        // [Authorize] // Usuário pode deletar a própria inscrição, ou admin
        public async Task<IActionResult> DeleteInscricao(string id)
        {
            var inscricao = await _context.Inscricoes.FindAsync(id);
            if (inscricao == null)
            {
                return NotFound(new { message = $"Inscrição com ID {id} não encontrada para exclusão." });
            }

            _context.Inscricoes.Remove(inscricao);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Erro ao excluir inscrição: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao excluir a inscrição no banco de dados." });
            }

            return NoContent();
        }

        private async Task<bool> InscricaoExistsAsync(string id)
        {
            return await _context.Inscricoes.AnyAsync(e => e.Id == id);
        }
    }
}
