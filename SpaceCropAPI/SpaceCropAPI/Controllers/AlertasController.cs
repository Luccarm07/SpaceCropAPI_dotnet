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
    [Route("alertas")]
    public class AlertasController : ControllerBase
    {
        private readonly SpaceCropContext _context;
        private readonly IMapper _mapper;
        private readonly SequenceHelper _seq;

        public AlertasController(SpaceCropContext context, IMapper mapper, SequenceHelper seq)
        {
            _context = context;
            _mapper  = mapper;
            _seq     = seq;
        }

        /// <summary>Lista alertas de uma fazenda com paginação, ordenados do mais recente.</summary>
        [HttpGet("/fazendas/{fazendaId}/alertas")]
        [ProducesResponseType(typeof(PagedResponseDTO<AlertaResponseDTO>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PagedResponseDTO<AlertaResponseDTO>>> GetByFazenda(
            long fazendaId,
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            var fazenda = await _context.Fazendas.FindAsync(fazendaId);
            if (fazenda == null)
                return NotFound(new { mensagem = $"Fazenda com ID {fazendaId} não encontrada." });

            var query = _context.Alertas
                .Include(a => a.TipoAlerta)
                .Include(a => a.Leitura)
                .Where(a => a.Leitura.IdFazenda == fazendaId)
                .OrderByDescending(a => a.DtAlerta);

            var total      = await query.CountAsync();
            var items      = await query.Skip(page * size).Take(size).ToListAsync();
            var totalPages = (int)Math.Ceiling((double)total / size);

            return Ok(new PagedResponseDTO<AlertaResponseDTO>
            {
                Content       = _mapper.Map<IEnumerable<AlertaResponseDTO>>(items),
                Page          = page,
                Size          = size,
                TotalElements = total,
                TotalPages    = totalPages,
                Last          = (page + 1) >= totalPages
            });
        }

        /// <summary>Lista todos os alertas com filtros opcionais de status e severidade.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponseDTO<AlertaResponseDTO>), 200)]
        public async Task<ActionResult<PagedResponseDTO<AlertaResponseDTO>>> GetAll(
            [FromQuery] bool?   resolvido  = null,
            [FromQuery] string? severidade = null,
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            var query = _context.Alertas
                .Include(a => a.TipoAlerta)
                .AsQueryable();

            if (resolvido.HasValue)
            {
                var flResolvido = resolvido.Value ? "S" : "N";
                query = query.Where(a => a.FlResolvido == flResolvido);
            }

            if (!string.IsNullOrEmpty(severidade))
            {
                var sev = severidade.ToUpper();
                query = query.Where(a => a.TipoAlerta.DsSeveridade == sev);
            }

            query = query.OrderByDescending(a => a.DtAlerta);

            var total      = await query.CountAsync();
            var items      = await query.Skip(page * size).Take(size).ToListAsync();
            var totalPages = (int)Math.Ceiling((double)total / size);

            return Ok(new PagedResponseDTO<AlertaResponseDTO>
            {
                Content       = _mapper.Map<IEnumerable<AlertaResponseDTO>>(items),
                Page          = page,
                Size          = size,
                TotalElements = total,
                TotalPages    = totalPages,
                Last          = (page + 1) >= totalPages
            });
        }

        /// <summary>Busca alerta por ID.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AlertaResponseDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AlertaResponseDTO>> GetById(long id)
        {
            var alerta = await _context.Alertas
                .Include(a => a.TipoAlerta)
                .FirstOrDefaultAsync(a => a.IdAlerta == id);

            if (alerta == null)
                return NotFound(new { mensagem = $"Alerta com ID {id} não encontrado." });

            return Ok(_mapper.Map<AlertaResponseDTO>(alerta));
        }

        // PUT /alertas/{id}/resolver
        /// <summary>Resolve um alerta registrando a ação tomada pelo usuário.</summary>
        [HttpPut("{id}/resolver")]
        [ProducesResponseType(typeof(AlertaResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AlertaResponseDTO>> Resolver(
            long id,
            [FromBody] ResolverAlertaRequestDTO request)
        {
            // Usa ADO.NET direto para evitar qualquer geração de SQL com TRUE/FALSE pelo EF Core
            var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            // 1. Verifica se usuário existe via SQL direto
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM TB_USUARIO WHERE ID_USUARIO = :p0";
                var p = cmd.CreateParameter();
                p.ParameterName = "p0";
                p.Value = request.UsuarioId;
                cmd.Parameters.Add(p);
                var count = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                if (count == 0)
                    return NotFound(new { mensagem = $"Usuário com ID {request.UsuarioId} não encontrado." });
            }

            // 2. Verifica se alerta existe e pega FL_RESOLVIDO via SQL direto
            string flResolvido;
            long idTipoAlerta;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT FL_RESOLVIDO, ID_TIPO_ALERTA FROM TB_ALERTA WHERE ID_ALERTA = :p0";
                var p = cmd.CreateParameter();
                p.ParameterName = "p0";
                p.Value = id;
                cmd.Parameters.Add(p);
                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return NotFound(new { mensagem = $"Alerta com ID {id} não encontrado." });
                flResolvido  = reader.GetString(0);
                idTipoAlerta = reader.GetInt64(1);
            }

            if (flResolvido == "S")
                return BadRequest(new { mensagem = "Este alerta já foi resolvido." });

            // 3. Próximo ID da sequence
            long idAcao;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT SEQ_ACAO.NEXTVAL FROM DUAL";
                idAcao = Convert.ToInt64(await cmd.ExecuteScalarAsync());
            }

            // 4. Executa UPDATE + INSERT numa transação ADO.NET
            using var dbTransaction = conn.BeginTransaction();
            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = dbTransaction;
                    cmd.CommandText = "UPDATE TB_ALERTA SET FL_RESOLVIDO = 'S' WHERE ID_ALERTA = :p0";
                    var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = id;
                    cmd.Parameters.Add(p);
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = dbTransaction;
                    cmd.CommandText = @"INSERT INTO TB_ACAO_ALERTA (ID_ACAO, ID_ALERTA, ID_USUARIO, DS_ACAO_TOMADA, DT_ACAO)
                                        VALUES (:p0, :p1, :p2, :p3, SYSDATE)";
                    var p0 = cmd.CreateParameter(); p0.ParameterName = "p0"; p0.Value = idAcao;
                    var p1 = cmd.CreateParameter(); p1.ParameterName = "p1"; p1.Value = id;
                    var p2 = cmd.CreateParameter(); p2.ParameterName = "p2"; p2.Value = request.UsuarioId;
                    var p3 = cmd.CreateParameter(); p3.ParameterName = "p3"; p3.Value = request.AcaoTomada;
                    cmd.Parameters.Add(p0); cmd.Parameters.Add(p1);
                    cmd.Parameters.Add(p2); cmd.Parameters.Add(p3);
                    await cmd.ExecuteNonQueryAsync();
                }

                dbTransaction.Commit();
            }
            catch
            {
                dbTransaction.Rollback();
                throw;
            }

            // 5. Busca o alerta atualizado para retornar (via EF Core, sem filtro booleano)
            var alertaAtualizado = await _context.Alertas
                .Include(a => a.TipoAlerta)
                .FirstOrDefaultAsync(a => a.IdAlerta == id);

            return Ok(_mapper.Map<AlertaResponseDTO>(alertaAtualizado));
        }
    }
}
