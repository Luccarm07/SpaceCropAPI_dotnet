using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceCropAPI.Data;
using SpaceCropAPI.DTOs;

namespace SpaceCropAPI.Controllers
{
    [ApiController]
    [Route("satelites")]
    public class SatelitesController : ControllerBase
    {
        private readonly SpaceCropContext _context;
        private readonly IMapper _mapper;

        public SatelitesController(SpaceCropContext context, IMapper mapper)
        {
            _context = context;
            _mapper  = mapper;
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /satelites
        // ─────────────────────────────────────────────────────────────────
        /// <summary>Lista todos os satélites com paginação.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponseDTO<SateliteResponseDTO>), 200)]
        public async Task<ActionResult<PagedResponseDTO<SateliteResponseDTO>>> GetAll(
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            var query = _context.Satelites.OrderBy(s => s.NmSatelite);

            var total      = await query.CountAsync();
            var items      = await query.Skip(page * size).Take(size).ToListAsync();
            var totalPages = (int)Math.Ceiling((double)total / size);

            return Ok(new PagedResponseDTO<SateliteResponseDTO>
            {
                Content       = _mapper.Map<IEnumerable<SateliteResponseDTO>>(items),
                Page          = page,
                Size          = size,
                TotalElements = total,
                TotalPages    = totalPages,
                Last          = (page + 1) >= totalPages
            });
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /satelites/{id}/sensores
        // ─────────────────────────────────────────────────────────────────
        /// <summary>Lista sensores orbitais de um satélite com paginação.</summary>
        [HttpGet("{id}/sensores")]
        [ProducesResponseType(typeof(PagedResponseDTO<SensorOrbitalResponseDTO>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PagedResponseDTO<SensorOrbitalResponseDTO>>> GetSensores(
            long id,
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            var satelite = await _context.Satelites.FindAsync(id);
            if (satelite == null)
                return NotFound(new { mensagem = $"Satélite com ID {id} não encontrado." });

            var query = _context.SensoresOrbitais
                .Include(s => s.TipoSensor)
                .Where(s => s.IdSatelite == id)
                .OrderBy(s => s.NmSensor);

            var total      = await query.CountAsync();
            var items      = await query.Skip(page * size).Take(size).ToListAsync();
            var totalPages = (int)Math.Ceiling((double)total / size);

            return Ok(new PagedResponseDTO<SensorOrbitalResponseDTO>
            {
                Content       = _mapper.Map<IEnumerable<SensorOrbitalResponseDTO>>(items),
                Page          = page,
                Size          = size,
                TotalElements = total,
                TotalPages    = totalPages,
                Last          = (page + 1) >= totalPages
            });
        }
    }
}
