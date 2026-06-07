using Microsoft.EntityFrameworkCore;
using SpaceCropAPI.Data;
using System.Data;

namespace SpaceCropAPI.Services
{
    public class SequenceHelper
    {
        private readonly SpaceCropContext _context;

        public SequenceHelper(SpaceCropContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém o próximo valor de uma sequence Oracle.
        /// Usa ADO.NET diretamente para evitar incompatibilidade do SqlQueryRaw com o provider Oracle EF Core.
        /// Reaproveita a conexão já aberta pelo DbContext se disponível.
        /// </summary>
        public async Task<long> NextValAsync(string sequenceName)
        {
            var conn = _context.Database.GetDbConnection();
            var wasOpen = conn.State == ConnectionState.Open;

            if (!wasOpen)
                await conn.OpenAsync();

            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT {sequenceName}.NEXTVAL FROM DUAL";
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt64(result);
            }
            finally
            {
                // Só fecha se foi a gente que abriu
                if (!wasOpen)
                    await conn.CloseAsync();
            }
        }
    }
}
