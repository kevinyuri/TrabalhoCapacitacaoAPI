using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoCapacitacao.Data;
using TrabalhoCapacitacao.DTOs.Vaga;
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
                Id = v.Id, // v.Id agora é Guid
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
    [HttpGet("{id:guid}")] // Adicionado constraint de rota para Guid
    public async Task<ActionResult<VagaResponseDto>> GetVaga(Guid id) // Parâmetro id agora é Guid
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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var vaga = new Vaga
        {
            // O Id NÃO é definido aqui. O banco de dados irá gerá-lo.
            Titulo = vagaCreateDto.Titulo,
            Descricao = vagaCreateDto.Descricao,
            Empresa = vagaCreateDto.Empresa,
            Local = vagaCreateDto.Local,
            TipoContrato = vagaCreateDto.TipoContrato,
            DataPublicacao = DateTime.UtcNow
        };

        _context.Vagas.Add(vaga);

        try
        {
            await _context.SaveChangesAsync();
            // Após SaveChangesAsync, vaga.Id será populado com o valor gerado pelo banco.
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"Erro ao salvar vaga: {ex.ToString()}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao salvar a vaga no banco de dados." });
        }

        var vagaResponseDto = new VagaResponseDto
        {
            Id = vaga.Id, // vaga.Id agora tem o valor gerado pelo DB
            Titulo = vaga.Titulo,
            Descricao = vaga.Descricao,
            Empresa = vaga.Empresa,
            Local = vaga.Local,
            TipoContrato = vaga.TipoContrato,
            DataPublicacao = vaga.DataPublicacao
        };

        // Retorna 201 Created com a localização do novo recurso e o recurso criado
        return CreatedAtAction(nameof(GetVaga), new { id = vaga.Id }, vagaResponseDto);
    }

    // PUT: api/Vagas/{id}
    [HttpPut("{id:guid}")] // Adicionado constraint de rota para Guid
    // [Authorize(Roles = "empresa,admin")]
    public async Task<IActionResult> PutVaga(Guid id, [FromBody] VagaUpdateDto vagaUpdateDto) // Parâmetro id agora é Guid
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

        vagaExistente.Titulo = vagaUpdateDto.Titulo;
        vagaExistente.Descricao = vagaUpdateDto.Descricao;
        vagaExistente.Empresa = vagaUpdateDto.Empresa;
        vagaExistente.Local = vagaUpdateDto.Local;
        vagaExistente.TipoContrato = vagaUpdateDto.TipoContrato;

        _context.Entry(vagaExistente).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine($"Erro de concorrência ao atualizar vaga: {ex.ToString()}");
            if (!await VagaExistsAsync(id))
            {
                return NotFound(new { message = $"Vaga com ID {id} não encontrada (concorrência)." });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro de concorrência ao atualizar a vaga." });
            }
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"Erro ao atualizar vaga: {ex.ToString()}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao atualizar a vaga no banco de dados." });
        }

        return NoContent();
    }

    // DELETE: api/Vagas/{id}
    [HttpDelete("{id:guid}")] // Adicionado constraint de rota para Guid
    // [Authorize(Roles = "empresa,admin")]
    public async Task<IActionResult> DeleteVaga(Guid id) // Parâmetro id agora é Guid
    {
        var vaga = await _context.Vagas.FindAsync(id);
        if (vaga == null)
        {
            return NotFound(new { message = $"Vaga com ID {id} não encontrada para exclusão." });
        }

        var inscricoesAssociadas = await _context.Inscricoes.AnyAsync(i => i.VagaId == id);
        if (inscricoesAssociadas)
        {
            return BadRequest(new { message = "Não é possível excluir a vaga pois existem inscrições associadas a ela." });
        }

        _context.Vagas.Remove(vaga);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"Erro ao excluir vaga: {ex.ToString()}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao excluir a vaga no banco de dados." });
        }

        return NoContent();
    }

    private async Task<bool> VagaExistsAsync(Guid id) // Parâmetro id agora é Guid
    {
        return await _context.Vagas.AnyAsync(e => e.Id == id);
    }
}
