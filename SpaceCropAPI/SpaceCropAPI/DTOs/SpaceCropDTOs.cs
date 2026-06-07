using System.ComponentModel.DataAnnotations;

namespace SpaceCropAPI.DTOs
{
    // ===================== USUARIO =====================

    public class UsuarioRequestDTO
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MaxLength(100)]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "Email é obrigatório.")]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "Senha deve ter ao menos 6 caracteres.")]
        public string Senha { get; set; } = null!;
    }

    public class UsuarioResponseDTO
    {
        public long Id { get; set; }
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    // ===================== FAZENDA =====================

    public class FazendaRequestDTO
    {
        [Required(ErrorMessage = "Nome da fazenda é obrigatório.")]
        [MaxLength(150)]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "Cidade é obrigatória.")]
        [MaxLength(100)]
        public string Cidade { get; set; } = null!;

        [Required(ErrorMessage = "Estado é obrigatório.")]
        [MaxLength(2, ErrorMessage = "Estado deve ter 2 caracteres (UF).")]
        [MinLength(2, ErrorMessage = "Estado deve ter 2 caracteres (UF).")]
        public string Estado { get; set; } = null!;

        [Required(ErrorMessage = "Área em hectares é obrigatória.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Área deve ser maior que zero.")]
        public decimal AreaHectares { get; set; }

        [Required(ErrorMessage = "ID do usuário é obrigatório.")]
        public long UsuarioId { get; set; }
    }

    public class FazendaResponseDTO
    {
        public long Id { get; set; }
        public long UsuarioId { get; set; }
        public string Nome { get; set; } = null!;
        public string Cidade { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public decimal AreaHectares { get; set; }
    }

    // ===================== SETOR PLANTIO =====================

    public class SetorRequestDTO
    {
        [Required(ErrorMessage = "Nome do setor é obrigatório.")]
        [MaxLength(100)]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "Cultura é obrigatória.")]
        [MaxLength(100)]
        public string Cultura { get; set; } = null!;

        [Required(ErrorMessage = "Área em hectares é obrigatória.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Área deve ser maior que zero.")]
        public decimal AreaHectares { get; set; }
    }

    public class SetorResponseDTO
    {
        public long Id { get; set; }
        public long FazendaId { get; set; }
        public string Nome { get; set; } = null!;
        public string Cultura { get; set; } = null!;
        public decimal AreaHectares { get; set; }
    }

    // ===================== SATELITE =====================

    public class SateliteResponseDTO
    {
        public long Id { get; set; }
        public string Nome { get; set; } = null!;
        public string Operador { get; set; } = null!;
        public bool Ativo { get; set; }
    }

    // ===================== SENSOR ORBITAL =====================

    public class SensorOrbitalResponseDTO
    {
        public long Id { get; set; }
        public long SateliteId { get; set; }
        public long TipoSensorId { get; set; }
        public string Nome { get; set; } = null!;
        public bool Ativo { get; set; }
        public string? TipoNome { get; set; }
        public string? UnidadeMedida { get; set; }
        public decimal? ValorCritico { get; set; }
    }

    // ===================== LEITURA =====================

    public class LeituraRequestDTO
    {
        [Required(ErrorMessage = "Sensor orbital é obrigatório.")]
        public long SensorOrbitalId { get; set; }

        [Required(ErrorMessage = "Fazenda é obrigatória.")]
        public long FazendaId { get; set; }

        public long? SetorId { get; set; }

        [Required(ErrorMessage = "Valor da leitura é obrigatório.")]
        public decimal Valor { get; set; }

        public DateTime? DataLeitura { get; set; }

        public bool Anomalia { get; set; } = false;
    }

    public class LeituraResponseDTO
    {
        public long Id { get; set; }
        public long SensorOrbitalId { get; set; }
        public long FazendaId { get; set; }
        public long? SetorId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataLeitura { get; set; }
        public bool Anomalia { get; set; }
        public string? SensorNome { get; set; }
        public string? UnidadeMedida { get; set; }
    }

    // ===================== ALERTA =====================

    public class AlertaRequestDTO
    {
        [Required(ErrorMessage = "ID da leitura é obrigatório.")]
        public long LeituraId { get; set; }

        [Required(ErrorMessage = "ID do tipo de alerta é obrigatório.")]
        public long TipoAlertaId { get; set; }

        [Required(ErrorMessage = "ID do usuário é obrigatório.")]
        public long UsuarioId { get; set; }
    }

    public class AlertaResponseDTO
    {
        public long Id { get; set; }
        public long LeituraId { get; set; }
        public long TipoAlertaId { get; set; }
        public long UsuarioId { get; set; }
        public bool Resolvido { get; set; }
        public DateTime DataAlerta { get; set; }
        public string? TipoNome { get; set; }
        public string? Severidade { get; set; }
    }

    public class ResolverAlertaRequestDTO
    {
        [Required(ErrorMessage = "ID do usuário que está resolvendo é obrigatório.")]
        public long UsuarioId { get; set; }

        [Required(ErrorMessage = "Descrição da ação tomada é obrigatória.")]
        [MaxLength(500)]
        public string AcaoTomada { get; set; } = null!;
    }

    // ===================== PAGINAÇÃO =====================

    public class PagedResponseDTO<T>
    {
        public IEnumerable<T> Content { get; set; } = new List<T>();
        public int Page { get; set; }
        public int Size { get; set; }
        public long TotalElements { get; set; }
        public int TotalPages { get; set; }
        public bool Last { get; set; }
    }
}
