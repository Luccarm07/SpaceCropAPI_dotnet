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
    [Route("leituras")]
    public class LeiturasController : ControllerBase
    {
        private readonly SpaceCropContext _context;
        private readonly IMapper _mapper;
        private readonly SequenceHelper _seq;

        public LeiturasController(SpaceCropContext context, IMapper mapper, SequenceHelper seq)
        {
            _context = context;
            _mapper  = mapper;
            _seq     = seq;
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /fazendas/{fazendaId}/leituras
        // ─────────────────────────────────────────────────────────────────
        /// <summary>Lista leituras da fazenda com paginação e ordenação.</summary>
        [HttpGet("/fazendas/{fazendaId}/leituras")]
        [ProducesResponseType(typeof(PagedResponseDTO<LeituraResponseDTO>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PagedResponseDTO<LeituraResponseDTO>>> GetByFazenda(
            long fazendaId,
            [FromQuery] string? orderBy  = "data",
            [FromQuery] string? orderDir = "desc",
            [FromQuery] bool?   anomalia = null,
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            var fazenda = await _context.Fazendas.FindAsync(fazendaId);
            if (fazenda == null)
                return NotFound(new { mensagem = $"Fazenda com ID {fazendaId} não encontrada." });

            var query = _context.LeiturasSatelite
                .Include(l => l.SensorOrbital).ThenInclude(s => s.TipoSensor)
                .Where(l => l.IdFazenda == fazendaId)
                .AsQueryable();

            if (anomalia.HasValue)
            {
                var flAnomalia = anomalia.Value ? "S" : "N";
                query = query.Where(l => l.FlAnomalia == flAnomalia);
            }

            query = (orderBy?.ToLower(), orderDir?.ToLower()) switch
            {
                ("valor", "asc") => query.OrderBy(l => l.NrValor),
                ("valor", _)     => query.OrderByDescending(l => l.NrValor),
                ("data",  "asc") => query.OrderBy(l => l.DtLeitura),
                _                => query.OrderByDescending(l => l.DtLeitura)
            };

            var total      = await query.CountAsync();
            var items      = await query.Skip(page * size).Take(size).ToListAsync();
            var totalPages = (int)Math.Ceiling((double)total / size);

            return Ok(new PagedResponseDTO<LeituraResponseDTO>
            {
                Content       = _mapper.Map<IEnumerable<LeituraResponseDTO>>(items),
                Page          = page,
                Size          = size,
                TotalElements = total,
                TotalPages    = totalPages,
                Last          = (page + 1) >= totalPages
            });
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /fazendas/{fazendaId}/leituras/ultimas
        // ─────────────────────────────────────────────────────────────────
        /// <summary>Últimas leituras da fazenda ordenadas da mais recente, com paginação.</summary>
        [HttpGet("/fazendas/{fazendaId}/leituras/ultimas")]
        [ProducesResponseType(typeof(PagedResponseDTO<LeituraResponseDTO>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PagedResponseDTO<LeituraResponseDTO>>> GetUltimas(
            long fazendaId,
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            var fazenda = await _context.Fazendas.FindAsync(fazendaId);
            if (fazenda == null)
                return NotFound(new { mensagem = $"Fazenda com ID {fazendaId} não encontrada." });

            var query = _context.LeiturasSatelite
                .Include(l => l.SensorOrbital).ThenInclude(s => s.TipoSensor)
                .Where(l => l.IdFazenda == fazendaId)
                .OrderByDescending(l => l.DtLeitura);

            var total      = await query.CountAsync();
            var items      = await query.Skip(page * size).Take(size).ToListAsync();
            var totalPages = (int)Math.Ceiling((double)total / size);

            return Ok(new PagedResponseDTO<LeituraResponseDTO>
            {
                Content       = _mapper.Map<IEnumerable<LeituraResponseDTO>>(items),
                Page          = page,
                Size          = size,
                TotalElements = total,
                TotalPages    = totalPages,
                Last          = (page + 1) >= totalPages
            });
        }

        // ─────────────────────────────────────────────────────────────────
        // POST /leituras
        // ─────────────────────────────────────────────────────────────────
        /// <summary>Insere nova leitura (simula coleta de satélite).</summary>
        [HttpPost]
        [ProducesResponseType(typeof(LeituraResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<LeituraResponseDTO>> Create([FromBody] LeituraRequestDTO request)
        {
            // AnyAsync gera TRUE/FALSE que o Oracle rejeita (ORA-00904) — usar COUNT via ADO.NET
            var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM TB_SENSOR_ORBITAL WHERE ID_SENSOR_ORBITAL = :p0";
                var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = request.SensorOrbitalId;
                cmd.Parameters.Add(p);
                if (Convert.ToInt64(await cmd.ExecuteScalarAsync()) == 0)
                    return NotFound(new { mensagem = $"Sensor orbital com ID {request.SensorOrbitalId} não encontrado." });
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM TB_FAZENDA WHERE ID_FAZENDA = :p0";
                var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = request.FazendaId;
                cmd.Parameters.Add(p);
                if (Convert.ToInt64(await cmd.ExecuteScalarAsync()) == 0)
                    return NotFound(new { mensagem = $"Fazenda com ID {request.FazendaId} não encontrada." });
            }

            if (request.SetorId.HasValue)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(1) FROM TB_SETOR_PLANTIO WHERE ID_SETOR = :p0";
                var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = request.SetorId.Value;
                cmd.Parameters.Add(p);
                if (Convert.ToInt64(await cmd.ExecuteScalarAsync()) == 0)
                    return NotFound(new { mensagem = $"Setor com ID {request.SetorId} não encontrado." });
            }

            var leitura = _mapper.Map<LeituraSatelite>(request);
            leitura.IdLeitura = await _seq.NextValAsync("SEQ_LEITURA");

            _context.LeiturasSatelite.Add(leitura);
            await _context.SaveChangesAsync();

            var leituraCompleta = await _context.LeiturasSatelite
                .Include(l => l.SensorOrbital).ThenInclude(s => s.TipoSensor)
                .FirstAsync(l => l.IdLeitura == leitura.IdLeitura);

            return CreatedAtAction(nameof(GetById), new { id = leitura.IdLeitura },
                _mapper.Map<LeituraResponseDTO>(leituraCompleta));
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /leituras/{id}
        // ─────────────────────────────────────────────────────────────────
        /// <summary>Busca leitura por ID.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LeituraResponseDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<LeituraResponseDTO>> GetById(long id)
        {
            var leitura = await _context.LeiturasSatelite
                .Include(l => l.SensorOrbital).ThenInclude(s => s.TipoSensor)
                .FirstOrDefaultAsync(l => l.IdLeitura == id);

            if (leitura == null)
                return NotFound(new { mensagem = $"Leitura com ID {id} não encontrada." });

            return Ok(_mapper.Map<LeituraResponseDTO>(leitura));
        }
    }
}
