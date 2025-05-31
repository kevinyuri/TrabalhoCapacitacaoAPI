using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoCapacitacao.Data;
using TrabalhoCapacitacao.DTOs.Curso;
using TrabalhoCapacitacao.Models;

namespace TrabalhoCapacitacao.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class CursosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CursosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Cursos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CursoResponseDto>>> GetCursos()
        {
            var cursos = await _context.Cursos
                .Select(c => new CursoResponseDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Instituicao = c.Instituicao,
                    CargaHoraria = c.CargaHoraria,
                    Modalidade = c.Modalidade,
                    DataInicio = c.DataInicio
                })
                .ToListAsync();
            return Ok(cursos);
        }

        // GET: api/Cursos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CursoResponseDto>> GetCurso(string id)
        {
            var curso = await _context.Cursos.FindAsync(id);

            if (curso == null)
            {
                return NotFound(new { message = $"Curso com ID {id} não encontrado." });
            }

            var cursoDto = new CursoResponseDto
            {
                Id = curso.Id,
                Nome = curso.Nome,
                Instituicao = curso.Instituicao,
                CargaHoraria = curso.CargaHoraria,
                Modalidade = curso.Modalidade,
                DataInicio = curso.DataInicio
            };

            return Ok(cursoDto);
        }

        // POST: api/Cursos
        [HttpPost]
        // [Authorize(Roles = "admin")] // Exemplo
        public async Task<ActionResult<CursoResponseDto>> PostCurso([FromBody] CursoCreateDto cursoCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var curso = new Curso
            {
                Nome = cursoCreateDto.Nome,
                Instituicao = cursoCreateDto.Instituicao,
                CargaHoraria = cursoCreateDto.CargaHoraria,
                Modalidade = cursoCreateDto.Modalidade,
                DataInicio = cursoCreateDto.DataInicio
            };

            _context.Cursos.Add(curso);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Verificar se o erro é de chave duplicada (improvável com GUID, mas bom ter)
                if (_context.Cursos.Any(e => e.Id == curso.Id)) // Checagem extra
                {
                    Console.WriteLine($"Erro de conflito ao salvar curso: {ex.ToString()}");
                    return Conflict(new { message = "Erro ao salvar o curso devido a um conflito de ID." });
                }
                Console.WriteLine($"Erro ao salvar curso: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao salvar o curso no banco de dados." });
            }

            var cursoResponseDto = new CursoResponseDto
            {
                Id = curso.Id,
                Nome = curso.Nome,
                Instituicao = curso.Instituicao,
                CargaHoraria = curso.CargaHoraria,
                Modalidade = curso.Modalidade,
                DataInicio = curso.DataInicio
            };

            return CreatedAtAction(nameof(GetCurso), new { id = curso.Id }, cursoResponseDto);
        }

        // PUT: api/Cursos/{id}
        [HttpPut("{id}")]
        // [Authorize(Roles = "admin")]
        public async Task<IActionResult> PutCurso(string id, [FromBody] CursoUpdateDto cursoUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cursoExistente = await _context.Cursos.FindAsync(id);

            if (cursoExistente == null)
            {
                return NotFound(new { message = $"Curso com ID {id} não encontrado para atualização." });
            }

            cursoExistente.Nome = cursoUpdateDto.Nome;
            cursoExistente.Instituicao = cursoUpdateDto.Instituicao;
            cursoExistente.CargaHoraria = cursoUpdateDto.CargaHoraria;
            cursoExistente.Modalidade = cursoUpdateDto.Modalidade;
            cursoExistente.DataInicio = cursoUpdateDto.DataInicio;

            _context.Entry(cursoExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"Erro de concorrência ao atualizar curso: {ex.ToString()}");
                if (!await CursoExistsAsync(id))
                {
                    return NotFound(new { message = $"Curso com ID {id} não encontrado (concorrência)." });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro de concorrência ao atualizar o curso." });
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Erro ao atualizar curso: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao atualizar o curso no banco de dados." });
            }

            return NoContent();
        }

        // DELETE: api/Cursos/{id}
        [HttpDelete("{id}")]
        // [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCurso(string id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound(new { message = $"Curso com ID {id} não encontrado para exclusão." });
            }

            var inscricoesAssociadas = await _context.Inscricoes.AnyAsync(i => i.CursoId == id);
            if (inscricoesAssociadas)
            {
                return BadRequest(new { message = "Não é possível excluir o curso pois existem inscrições associadas a ele." });
            }

            _context.Cursos.Remove(curso);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Erro ao excluir curso: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao excluir o curso no banco de dados." });
            }

            return NoContent();
        }

        private async Task<bool> CursoExistsAsync(string id)
        {
            return await _context.Cursos.AnyAsync(e => e.Id == id);
        }
    }
}
