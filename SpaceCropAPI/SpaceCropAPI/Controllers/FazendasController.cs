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
    [Route("fazendas")]
    public class FazendasController : ControllerBase
    {
        private readonly SpaceCropContext _context;
        private readonly IMapper _mapper;
        private readonly SequenceHelper _seq;

        public FazendasController(SpaceCropContext context, IMapper mapper, SequenceHelper seq)
        {
            _context = context;
            _mapper  = mapper;
            _seq     = seq;
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /fazendas  (filtra por usuarioId query param)
        // ─────────────────────────────────────────────────────────────────
        /// <summary>Lista fazendas com paginação. Filtra por usuarioId se informado.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponseDTO<FazendaResponseDTO>), 200)]
        public async Task<ActionResult<PagedResponseDTO<FazendaResponseDTO>>> GetAll(
            [FromQuery] long? usuarioId,
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            var query = _context.Fazendas.AsQueryable();

            if (usuarioId.HasValue)
                query = query.Where(f => f.IdUsuario == usuarioId.Value);

            query = query.OrderBy(f => f.NmFazenda);

            var total      = await query.CountAsync();
            var items      = await query.Skip(page * size).Take(size).ToListAsync();
            var totalPages = (int)Math.Ceiling((double)total / size);

            return Ok(new PagedResponseDTO<FazendaResponseDTO>
            {
                Content       = _mapper.Map<IEnumerable<FazendaResponseDTO>>(items),
                Page          = page,
                Size          = size,
                TotalElements = total,
                TotalPages    = totalPages,
                Last          = (page + 1) >= totalPages
            });
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /fazendas/{id}
        // ─────────────────────────────────────────────────────────────────
        /// <summary>Busca fazenda por ID.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FazendaResponseDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<FazendaResponseDTO>> GetById(long id)
        {
            var fazenda = await _context.Fazendas.FindAsync(id);

            if (fazenda == null)
                return NotFound(new { mensagem = $"Fazenda com ID {id} não encontrada." });

            return Ok(_mapper.Map<FazendaResponseDTO>(fazenda));
        }

        // ─────────────────────────────────────────────────────────────────
        // POST /fazendas
        // ─────────────────────────────────────────────────────────────────
        /// <summary>Cadastra nova fazenda. O campo usuarioId no body define o dono.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(FazendaResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<FazendaResponseDTO>> Create([FromBody] FazendaRequestDTO request)
        {
            // AnyAsync gera TRUE/FALSE que o Oracle rejeita (ORA-00904) — usar COUNT via ADO.NET
            var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM TB_USUARIO WHERE ID_USUARIO = :p0";
                var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = request.UsuarioId;
                cmd.Parameters.Add(p);
                var count = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                if (count == 0)
                    return NotFound(new { mensagem = $"Usuário com ID {request.UsuarioId} não encontrado." });
            }

            var fazenda = _mapper.Map<Fazenda>(request);
            fazenda.IdFazenda = await _seq.NextValAsync("SEQ_FAZENDA");

            _context.Fazendas.Add(fazenda);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = fazenda.IdFazenda },
                _mapper.Map<FazendaResponseDTO>(fazenda));
        }

        // ─────────────────────────────────────────────────────────────────
        // PUT /fazendas/{id}
        // ─────────────────────────────────────────────────────────────────
        /// <summary>Atualiza fazenda.</summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(FazendaResponseDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<FazendaResponseDTO>> Update(long id, [FromBody] FazendaRequestDTO request)
        {
            var fazenda = await _context.Fazendas.FindAsync(id);

            if (fazenda == null)
                return NotFound(new { mensagem = $"Fazenda com ID {id} não encontrada." });

            fazenda.NmFazenda      = request.Nome;
            fazenda.DsCidade       = request.Cidade;
            fazenda.DsEstado       = request.Estado;
            fazenda.NrAreaHectares = request.AreaHectares;

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<FazendaResponseDTO>(fazenda));
        }

        // ─────────────────────────────────────────────────────────────────
        // DELETE /fazendas/{id}
        // ─────────────────────────────────────────────────────────────────
        /// <summary>Remove fazenda e todos os seus registros dependentes.</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(long id)
        {
            // Usar ADO.NET direto em transação para garantir a ordem de deleção correta no Oracle
            var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            // Verifica se fazenda existe
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM TB_FAZENDA WHERE ID_FAZENDA = :p0";
                var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = id;
                cmd.Parameters.Add(p);
                if (Convert.ToInt64(await cmd.ExecuteScalarAsync()) == 0)
                    return NotFound(new { mensagem = $"Fazenda com ID {id} não encontrada." });
            }

            using var tx = conn.BeginTransaction();
            try
            {
                // 1. TB_ACAO_ALERTA (filho de TB_ALERTA, filho de TB_LEITURA_SATELITE)
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"DELETE FROM TB_ACAO_ALERTA
                                        WHERE ID_ALERTA IN (
                                            SELECT a.ID_ALERTA FROM TB_ALERTA a
                                            JOIN TB_LEITURA_SATELITE l ON l.ID_LEITURA = a.ID_LEITURA
                                            WHERE l.ID_FAZENDA = :p0
                                               OR l.ID_SETOR IN (SELECT ID_SETOR FROM TB_SETOR_PLANTIO WHERE ID_FAZENDA = :p1))";
                    var p0 = cmd.CreateParameter(); p0.ParameterName = "p0"; p0.Value = id;
                    var p1 = cmd.CreateParameter(); p1.ParameterName = "p1"; p1.Value = id;
                    cmd.Parameters.Add(p0);
                    cmd.Parameters.Add(p1);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2. TB_ALERTA
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"DELETE FROM TB_ALERTA
                                        WHERE ID_LEITURA IN (
                                            SELECT ID_LEITURA FROM TB_LEITURA_SATELITE
                                            WHERE ID_FAZENDA = :p0
                                               OR ID_SETOR IN (SELECT ID_SETOR FROM TB_SETOR_PLANTIO WHERE ID_FAZENDA = :p1))";
                    var p0 = cmd.CreateParameter(); p0.ParameterName = "p0"; p0.Value = id;
                    var p1 = cmd.CreateParameter(); p1.ParameterName = "p1"; p1.Value = id;
                    cmd.Parameters.Add(p0);
                    cmd.Parameters.Add(p1);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 3. TB_LEITURA_SATELITE
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"DELETE FROM TB_LEITURA_SATELITE
                                        WHERE ID_FAZENDA = :p0
                                           OR ID_SETOR IN (SELECT ID_SETOR FROM TB_SETOR_PLANTIO WHERE ID_FAZENDA = :p1)";
                    var p0 = cmd.CreateParameter(); p0.ParameterName = "p0"; p0.Value = id;
                    var p1 = cmd.CreateParameter(); p1.ParameterName = "p1"; p1.Value = id;
                    cmd.Parameters.Add(p0);
                    cmd.Parameters.Add(p1);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 4. TB_SETOR_PLANTIO
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = "DELETE FROM TB_SETOR_PLANTIO WHERE ID_FAZENDA = :p0";
                    var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = id;
                    cmd.Parameters.Add(p);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 5. TB_FAZENDA
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = "DELETE FROM TB_FAZENDA WHERE ID_FAZENDA = :p0";
                    var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = id;
                    cmd.Parameters.Add(p);
                    await cmd.ExecuteNonQueryAsync();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            return NoContent();
        }
    }
}
