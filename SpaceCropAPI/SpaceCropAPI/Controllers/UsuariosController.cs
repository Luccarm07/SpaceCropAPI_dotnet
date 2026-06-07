using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceCropAPI.Data;
using SpaceCropAPI.DTOs;
using SpaceCropAPI.Models;
using SpaceCropAPI.Services;
using System.Security.Cryptography;
using System.Text;

namespace SpaceCropAPI.Controllers
{
    [ApiController]
    [Route("usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly SpaceCropContext _context;
        private readonly IMapper _mapper;
        private readonly SequenceHelper _seq;

        public UsuariosController(SpaceCropContext context, IMapper mapper, SequenceHelper seq)
        {
            _context = context;
            _mapper  = mapper;
            _seq     = seq;
        }

        private static string HashSenha(string senha)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
            return Convert.ToHexString(bytes).ToLower();
        }

        /// <summary>Lista todos os usuários com paginação, ordenação e busca por nome.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponseDTO<UsuarioResponseDTO>), 200)]
        public async Task<ActionResult<PagedResponseDTO<UsuarioResponseDTO>>> GetAll(
            [FromQuery] string? nome,
            [FromQuery] string? orderBy  = "nome",
            [FromQuery] string? orderDir = "asc",
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            var query = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrEmpty(nome))
            {
                // .Contains() gera LIKE com ESCAPE inválido no Oracle (ORA-01425) — usar EF.Functions.Like
                var pattern = $"%{nome.ToUpper()}%";
                query = query.Where(u => EF.Functions.Like(u.NmUsuario.ToUpper(), pattern));
            }

            query = (orderBy?.ToLower(), orderDir?.ToLower()) switch
            {
                ("email", "desc") => query.OrderByDescending(u => u.DsEmail),
                ("email", _)      => query.OrderBy(u => u.DsEmail),
                ("nome",  "desc") => query.OrderByDescending(u => u.NmUsuario),
                _                 => query.OrderBy(u => u.NmUsuario)
            };

            var total      = await query.CountAsync();
            var items      = await query.Skip(page * size).Take(size).ToListAsync();
            var totalPages = (int)Math.Ceiling((double)total / size);

            return Ok(new PagedResponseDTO<UsuarioResponseDTO>
            {
                Content       = _mapper.Map<IEnumerable<UsuarioResponseDTO>>(items),
                Page          = page,
                Size          = size,
                TotalElements = total,
                TotalPages    = totalPages,
                Last          = (page + 1) >= totalPages
            });
        }

        /// <summary>Busca usuário por ID.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UsuarioResponseDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UsuarioResponseDTO>> GetById(long id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound(new { mensagem = $"Usuário com ID {id} não encontrado." });

            return Ok(_mapper.Map<UsuarioResponseDTO>(usuario));
        }

        /// <summary>Cadastra novo usuário.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(UsuarioResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<UsuarioResponseDTO>> Create([FromBody] UsuarioRequestDTO request)
        {
            // Verifica email duplicado via ADO.NET (AnyAsync gera TRUE/FALSE inválido no Oracle)
            var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM TB_USUARIO WHERE DS_EMAIL = :p0";
                var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = request.Email;
                cmd.Parameters.Add(p);
                if (Convert.ToInt64(await cmd.ExecuteScalarAsync()) > 0)
                    return Conflict(new { mensagem = "Este email já está em uso." });
            }

            var usuario = new Models.Usuario
            {
                IdUsuario   = await _seq.NextValAsync("SEQ_USUARIO"),
                NmUsuario   = request.Nome,
                DsEmail     = request.Email,
                DsSenhaHash = HashSenha(request.Senha)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = usuario.IdUsuario },
                _mapper.Map<UsuarioResponseDTO>(usuario));
        }

        /// <summary>Atualiza dados do usuário.</summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UsuarioResponseDTO), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<UsuarioResponseDTO>> Update(long id, [FromBody] UsuarioRequestDTO request)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound(new { mensagem = $"Usuário com ID {id} não encontrado." });

            if (!string.IsNullOrEmpty(request.Email) && request.Email != usuario.DsEmail)
            {
                // AnyAsync gera TRUE/FALSE que o Oracle rejeita (ORA-00904) — usar COUNT via ADO.NET
                var conn2 = _context.Database.GetDbConnection();
                if (conn2.State != System.Data.ConnectionState.Open)
                    await conn2.OpenAsync();
                using var cmd2 = conn2.CreateCommand();
                cmd2.CommandText = "SELECT COUNT(1) FROM TB_USUARIO WHERE DS_EMAIL = :p0 AND ID_USUARIO != :p1";
                var pe = cmd2.CreateParameter(); pe.ParameterName = "p0"; pe.Value = request.Email; cmd2.Parameters.Add(pe);
                var pi = cmd2.CreateParameter(); pi.ParameterName = "p1"; pi.Value = id; cmd2.Parameters.Add(pi);
                if (Convert.ToInt64(await cmd2.ExecuteScalarAsync()) > 0)
                    return Conflict(new { mensagem = "Este email já está em uso por outro usuário." });
            }

            usuario.NmUsuario   = request.Nome;
            usuario.DsEmail     = request.Email;
            usuario.DsSenhaHash = HashSenha(request.Senha);

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<UsuarioResponseDTO>(usuario));
        }

        /// <summary>Remove usuário e todos os seus registros dependentes.</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(long id)
        {
            var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            // Verifica se usuário existe
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM TB_USUARIO WHERE ID_USUARIO = :p0";
                var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = id;
                cmd.Parameters.Add(p);
                if (Convert.ToInt64(await cmd.ExecuteScalarAsync()) == 0)
                    return NotFound(new { mensagem = $"Usuário com ID {id} não encontrado." });
            }

            using var tx = conn.BeginTransaction();
            try
            {
                // ═══════════════════════════════════════════════════════════════
                // BLOCO A — dependentes das leituras das fazendas deste usuário
                // ═══════════════════════════════════════════════════════════════

                // 1. TB_ACAO_ALERTA via alertas de leituras das fazendas do usuário
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"DELETE FROM TB_ACAO_ALERTA
                                        WHERE ID_ALERTA IN (
                                            SELECT a.ID_ALERTA FROM TB_ALERTA a
                                            JOIN TB_LEITURA_SATELITE l ON l.ID_LEITURA = a.ID_LEITURA
                                            JOIN TB_FAZENDA f ON f.ID_FAZENDA = l.ID_FAZENDA
                                            WHERE f.ID_USUARIO = :p0)";
                    var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = id;
                    cmd.Parameters.Add(p);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2. TB_ALERTA via leituras das fazendas do usuário
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"DELETE FROM TB_ALERTA
                                        WHERE ID_LEITURA IN (
                                            SELECT l.ID_LEITURA FROM TB_LEITURA_SATELITE l
                                            JOIN TB_FAZENDA f ON f.ID_FAZENDA = l.ID_FAZENDA
                                            WHERE f.ID_USUARIO = :p0)";
                    var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = id;
                    cmd.Parameters.Add(p);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 3. TB_LEITURA_SATELITE das fazendas do usuário
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"DELETE FROM TB_LEITURA_SATELITE
                                        WHERE ID_FAZENDA IN (
                                            SELECT ID_FAZENDA FROM TB_FAZENDA
                                            WHERE ID_USUARIO = :p0)";
                    var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = id;
                    cmd.Parameters.Add(p);
                    await cmd.ExecuteNonQueryAsync();
                }

                // ═══════════════════════════════════════════════════════════════
                // BLOCO B — leituras de outras fazendas que referenciam setores
                //           das fazendas deste usuário (referências cruzadas)
                // ═══════════════════════════════════════════════════════════════

                // 4. TB_ACAO_ALERTA via alertas de leituras cruzadas nos setores do usuário
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"DELETE FROM TB_ACAO_ALERTA
                                        WHERE ID_ALERTA IN (
                                            SELECT a.ID_ALERTA FROM TB_ALERTA a
                                            JOIN TB_LEITURA_SATELITE l ON l.ID_LEITURA = a.ID_LEITURA
                                            JOIN TB_SETOR_PLANTIO    s ON s.ID_SETOR   = l.ID_SETOR
                                            JOIN TB_FAZENDA          f ON f.ID_FAZENDA = s.ID_FAZENDA
                                            WHERE f.ID_USUARIO = :p0)";
                    var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = id;
                    cmd.Parameters.Add(p);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 5. TB_ALERTA via leituras cruzadas nos setores do usuário
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"DELETE FROM TB_ALERTA
                                        WHERE ID_LEITURA IN (
                                            SELECT l.ID_LEITURA FROM TB_LEITURA_SATELITE l
                                            JOIN TB_SETOR_PLANTIO s ON s.ID_SETOR   = l.ID_SETOR
                                            JOIN TB_FAZENDA       f ON f.ID_FAZENDA = s.ID_FAZENDA
                                            WHERE f.ID_USUARIO = :p0)";
                    var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = id;
                    cmd.Parameters.Add(p);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 6. TB_LEITURA_SATELITE de outras fazendas com id_setor dos setores do usuário
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"DELETE FROM TB_LEITURA_SATELITE
                                        WHERE ID_SETOR IN (
                                            SELECT s.ID_SETOR FROM TB_SETOR_PLANTIO s
                                            JOIN TB_FAZENDA f ON f.ID_FAZENDA = s.ID_FAZENDA
                                            WHERE f.ID_USUARIO = :p0)";
                    var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = id;
                    cmd.Parameters.Add(p);
                    await cmd.ExecuteNonQueryAsync();
                }

                // ═══════════════════════════════════════════════════════════════
                // BLOCO C — remove entidades do usuário (sem dependentes)
                // ═══════════════════════════════════════════════════════════════

                // 7. TB_SETOR_PLANTIO das fazendas do usuário
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"DELETE FROM TB_SETOR_PLANTIO
                                        WHERE ID_FAZENDA IN (
                                            SELECT ID_FAZENDA FROM TB_FAZENDA
                                            WHERE ID_USUARIO = :p0)";
                    var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = id;
                    cmd.Parameters.Add(p);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 8. TB_FAZENDA do usuário
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = "DELETE FROM TB_FAZENDA WHERE ID_USUARIO = :p0";
                    var p = cmd.CreateParameter(); p.ParameterName = "p0"; p.Value = id;
                    cmd.Parameters.Add(p);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 9. TB_USUARIO
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = "DELETE FROM TB_USUARIO WHERE ID_USUARIO = :p0";
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
