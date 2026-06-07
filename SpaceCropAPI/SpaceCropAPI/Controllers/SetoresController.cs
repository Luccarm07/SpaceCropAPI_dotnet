using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceCropAPI.Data;
using SpaceCropAPI.DTOs;
using SpaceCropAPI.Models;
using SpaceCropAPI.Services;

namespace SpaceCropAPI.Controllers
{
    [ApiController]
    [Route("setores")]
    public class SetoresController : ControllerBase
    {
        private readonly SpaceCropContext _context;
        private readonly IMapper _mapper;
        private readonly SequenceHelper _seq;

        public SetoresController(SpaceCropContext context, IMapper mapper, SequenceHelper seq)
        {
            _context = context;
            _mapper  = mapper;
            _seq     = seq;
        }

        /// <summary>Lista setores da fazenda com paginação.</summary>
        [HttpGet("/fazendas/{fazendaId}/setores")]
        [ProducesResponseType(typeof(PagedResponseDTO<SetorResponseDTO>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PagedResponseDTO<SetorResponseDTO>>> GetByFazenda(
            long fazendaId,
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            var fazenda = await _context.Fazendas.FindAsync(fazendaId);
            if (fazenda == null)
                return NotFound(new { mensagem = $"Fazenda com ID {fazendaId} não encontrada." });

            var query = _context.SetoresPlantio
                .Where(s => s.IdFazenda == fazendaId)
                .OrderBy(s => s.NmSetor);

            var total      = await query.CountAsync();
            var items      = await query.Skip(page * size).Take(size).ToListAsync();
            var totalPages = (int)Math.Ceiling((double)total / size);

            return Ok(new PagedResponseDTO<SetorResponseDTO>
            {
                Content       = _mapper.Map<IEnumerable<SetorResponseDTO>>(items),
                Page          = page,
                Size          = size,
                TotalElements = total,
                TotalPages    = totalPages,
                Last          = (page + 1) >= totalPages
            });
        }

        /// <summary>Cadastra setor em uma fazenda.</summary>
        [HttpPost("/fazendas/{fazendaId}/setores")]
        [ProducesResponseType(typeof(SetorResponseDTO), 201)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SetorResponseDTO>> Create(long fazendaId, [FromBody] SetorRequestDTO request)
        {
            var fazenda = await _context.Fazendas.FindAsync(fazendaId);
            if (fazenda == null)
                return NotFound(new { mensagem = $"Fazenda com ID {fazendaId} não encontrada." });

            var setor = _mapper.Map<SetorPlantio>(request);
            setor.IdSetor   = await _seq.NextValAsync("SEQ_SETOR");
            setor.IdFazenda = fazendaId;

            _context.SetoresPlantio.Add(setor);
            await _context.SaveChangesAsync();

            // Retorna 201 com Location header apontando para PUT /setores/{id}
            return Created($"/setores/{setor.IdSetor}", _mapper.Map<SetorResponseDTO>(setor));
        }

        /// <summary>Atualiza setor.</summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SetorResponseDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SetorResponseDTO>> Update(long id, [FromBody] SetorRequestDTO request)
        {
            var setor = await _context.SetoresPlantio.FindAsync(id);
            if (setor == null)
                return NotFound(new { mensagem = $"Setor com ID {id} não encontrado." });

            setor.NmSetor        = request.Nome;
            setor.DsCultura      = request.Cultura;
            setor.NrAreaHectares = request.AreaHectares;

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<SetorResponseDTO>(setor));
        }

        /// <summary>Remove setor (desvincula leituras associadas).</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(long id)
        {
            var setor = await _context.SetoresPlantio.FindAsync(id);
            if (setor == null)
                return NotFound(new { mensagem = $"Setor com ID {id} não encontrado." });

            var leituras = await _context.LeiturasSatelite
                .Where(l => l.IdSetor == id).ToListAsync();
            foreach (var leitura in leituras)
                leitura.IdSetor = null;

            _context.SetoresPlantio.Remove(setor);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
