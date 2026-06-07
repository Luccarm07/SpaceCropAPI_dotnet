using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceCropAPI.Data;
using SpaceCropAPI.DTOs;

namespace SpaceCropAPI.Controllers
{
    [ApiController]
    [Route("sensores")]
    public class SensoresController : ControllerBase
    {
        private readonly SpaceCropContext _context;
        private readonly IMapper _mapper;

        public SensoresController(SpaceCropContext context, IMapper mapper)
        {
            _context = context;
            _mapper  = mapper;
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /sensores  (busca por nome ou tipo, filtro ativo)
        // ─────────────────────────────────────────────────────────────────
        /// <summary>Lista todos os sensores orbitais com busca por nome ou tipo.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponseDTO<SensorOrbitalResponseDTO>), 200)]
        public async Task<ActionResult<PagedResponseDTO<SensorOrbitalResponseDTO>>> GetAll(
            [FromQuery] string? busca,
            [FromQuery] bool?   ativo,
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            var query = _context.SensoresOrbitais
                .Include(s => s.TipoSensor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busca))
            {
                // .Contains() gera LIKE com ESCAPE inválido no Oracle (ORA-01425) — usar EF.Functions.Like
                var pattern = $"%{busca.ToUpper()}%";
                query = query.Where(s =>
                    EF.Functions.Like(s.NmSensor.ToUpper(), pattern) ||
                    EF.Functions.Like(s.TipoSensor.NmTipo.ToUpper(), pattern));
            }

            if (ativo.HasValue)
            {
                var flAtivo = ativo.Value ? "S" : "N";
                query = query.Where(s => s.FlAtivo == flAtivo);
            }

            query = query.OrderBy(s => s.NmSensor);

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

        // ─────────────────────────────────────────────────────────────────
        // GET /sensores/{id}
        // ─────────────────────────────────────────────────────────────────
        /// <summary>Busca sensor orbital por ID.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SensorOrbitalResponseDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SensorOrbitalResponseDTO>> GetById(long id)
        {
            var sensor = await _context.SensoresOrbitais
                .Include(s => s.TipoSensor)
                .FirstOrDefaultAsync(s => s.IdSensorOrbital == id);

            if (sensor == null)
                return NotFound(new { mensagem = $"Sensor com ID {id} não encontrado." });

            return Ok(_mapper.Map<SensorOrbitalResponseDTO>(sensor));
        }
    }
}
