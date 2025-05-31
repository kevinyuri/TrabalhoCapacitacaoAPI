namespace TrabalhoCapacitacao.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoCapacitacao.Data;
using TrabalhoCapacitacao.DTOs.Vaga;
using TrabalhoCapacitacao.Models;

// using Microsoft.AspNetCore.Authorization;

// Certifique-se de que os namespaces dos seus DTOs e Modelos estão corretos
// Ex: using SeuProjeto.Models;
// Ex: using SeuProjeto.Dtos.VagaDtos;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class VagasController : ControllerBase
{
    private readonly AppDbContext _context;

    public VagasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Vagas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VagaResponseDto>>> GetVagas()
    {
        var vagas = await _context.Vagas
            .Select(v => new VagaResponseDto
            {
                Id = v.Id,
                Titulo = v.Titulo,
                Descricao = v.Descricao,
                Empresa = v.Empresa,
                Local = v.Local,
                TipoContrato = v.TipoContrato,
                DataPublicacao = v.DataPublicacao
            })
            .ToListAsync();
        return Ok(vagas);
    }

    // GET: api/Vagas/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<VagaResponseDto>> GetVaga(string id)
    {
        var vaga = await _context.Vagas.FindAsync(id);

        if (vaga == null)
        {
            return NotFound(new { message = $"Vaga com ID {id} não encontrada." });
        }

        var vagaDto = new VagaResponseDto
        {
            Id = vaga.Id,
            Titulo = vaga.Titulo,
            Descricao = vaga.Descricao,
            Empresa = vaga.Empresa,
            Local = vaga.Local,
            TipoContrato = vaga.TipoContrato,
            DataPublicacao = vaga.DataPublicacao
        };

        return Ok(vagaDto);
    }

    // POST: api/Vagas
    [HttpPost]
    // [Authorize(Roles = "empresa,admin")]
    public async Task<ActionResult<VagaResponseDto>> PostVaga([FromBody] VagaCreateDto vagaCreateDto)
    {
        // ModelState é preenchido automaticamente com erros de validação dos DataAnnotations
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var vaga = new Vaga
        {
            Titulo = vagaCreateDto.Titulo,
            Descricao = vagaCreateDto.Descricao,
            Empresa = vagaCreateDto.Empresa,
            Local = vagaCreateDto.Local,
            TipoContrato = vagaCreateDto.TipoContrato,
            DataPublicacao = DateTime.UtcNow // Definir data de publicação no servidor
        };

        _context.Vagas.Add(vaga);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Logar o erro ex
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao salvar a vaga no banco de dados." });
        }

        var vagaResponseDto = new VagaResponseDto
        {
            Id = vaga.Id,
            Titulo = vaga.Titulo,
            Descricao = vaga.Descricao,
            Empresa = vaga.Empresa,
            Local = vaga.Local,
            TipoContrato = vaga.TipoContrato,
            DataPublicacao = vaga.DataPublicacao
        };

        return CreatedAtAction(nameof(GetVaga), new { id = vaga.Id }, vagaResponseDto);
    }

    // PUT: api/Vagas/{id}
    [HttpPut("{id}")]
    // [Authorize(Roles = "empresa,admin")]
    public async Task<IActionResult> PutVaga(string id, [FromBody] VagaUpdateDto vagaUpdateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var vagaExistente = await _context.Vagas.FindAsync(id);

        if (vagaExistente == null)
        {
            return NotFound(new { message = $"Vaga com ID {id} não encontrada para atualização." });
        }

        // Mapear os campos do DTO para a entidade existente
        vagaExistente.Titulo = vagaUpdateDto.Titulo;
        vagaExistente.Descricao = vagaUpdateDto.Descricao;
        vagaExistente.Empresa = vagaUpdateDto.Empresa;
        vagaExistente.Local = vagaUpdateDto.Local;
        vagaExistente.TipoContrato = vagaUpdateDto.TipoContrato;
        // vagaExistente.DataPublicacao = DateTime.UtcNow; // Se quiser atualizar a data a cada modificação

        _context.Entry(vagaExistente).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Logar o erro ex
            if (!VagaExists(id))
            {
                return NotFound(new { message = $"Vaga com ID {id} não encontrada durante a tentativa de salvar (concorrência)." });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro de concorrência ao atualizar a vaga." });
            }
        }
        catch (DbUpdateException ex)
        {
            // Logar o erro ex
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao atualizar a vaga no banco de dados." });
        }

        return NoContent(); // Sucesso, sem conteúdo para retornar
    }

    // DELETE: api/Vagas/{id}
    [HttpDelete("{id}")]
    // [Authorize(Roles = "empresa,admin")]
    public async Task<IActionResult> DeleteVaga(string id)
    {
        var vaga = await _context.Vagas.FindAsync(id);
        if (vaga == null)
        {
            return NotFound(new { message = $"Vaga com ID {id} não encontrada para exclusão." });
        }

        var inscricoesAssociadas = await _context.Inscricoes.AnyAsync(i => i.VagaId == id);
        if (inscricoesAssociadas)
        {
            return BadRequest(new { message = "Não é possível excluir a vaga pois existem inscrições associadas a ela. Remova as inscrições primeiro." });
        }

        _context.Vagas.Remove(vaga);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Logar o erro ex
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao excluir a vaga no banco de dados." });
        }

        return NoContent();
    }

    private bool VagaExists(string id)
    {
        return _context.Vagas.Any(e => e.Id == id);
    }
}
