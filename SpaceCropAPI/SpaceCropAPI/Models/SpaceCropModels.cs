using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceCropAPI.Models
{
    [Table("TB_USUARIO")]
    public class Usuario
    {
        [Key]
        [Column("ID_USUARIO")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdUsuario { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("NM_USUARIO")]
        public string NmUsuario { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        [Column("DS_EMAIL")]
        public string DsEmail { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        [Column("DS_SENHA_HASH")]
        public string DsSenhaHash { get; set; } = null!;

        public ICollection<Fazenda> Fazendas { get; set; } = new List<Fazenda>();
        public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
        public ICollection<AcaoAlerta> AcoesAlerta { get; set; } = new List<AcaoAlerta>();
    }

    [Table("TB_FAZENDA")]
    public class Fazenda
    {
        [Key]
        [Column("ID_FAZENDA")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdFazenda { get; set; }

        [Required]
        [Column("ID_USUARIO")]
        public long IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        [Column("NM_FAZENDA")]
        public string NmFazenda { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [Column("DS_CIDADE")]
        public string DsCidade { get; set; } = null!;

        [Required]
        [MaxLength(2)]
        [Column("DS_ESTADO")]
        public string DsEstado { get; set; } = null!;

        [Required]
        [Column("NR_AREA_HECTARES")]
        public decimal NrAreaHectares { get; set; }

        public ICollection<SetorPlantio> SetoresPlantio { get; set; } = new List<SetorPlantio>();
        public ICollection<LeituraSatelite> Leituras { get; set; } = new List<LeituraSatelite>();
    }

    [Table("TB_SETOR_PLANTIO")]
    public class SetorPlantio
    {
        [Key]
        [Column("ID_SETOR")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdSetor { get; set; }

        [Required]
        [Column("ID_FAZENDA")]
        public long IdFazenda { get; set; }

        [ForeignKey("IdFazenda")]
        public Fazenda Fazenda { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [Column("NM_SETOR")]
        public string NmSetor { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [Column("DS_CULTURA")]
        public string DsCultura { get; set; } = null!;

        [Required]
        [Column("NR_AREA_HECTARES")]
        public decimal NrAreaHectares { get; set; }

        public ICollection<LeituraSatelite> Leituras { get; set; } = new List<LeituraSatelite>();
    }

    [Table("TB_SATELITE")]
    public class Satelite
    {
        [Key]
        [Column("ID_SATELITE")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdSatelite { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("NM_SATELITE")]
        public string NmSatelite { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [Column("DS_OPERADOR")]
        public string DsOperador { get; set; } = null!;

        [Required]
        [MaxLength(1)]
        [Column("FL_ATIVO")]
        public string FlAtivo { get; set; } = "S";

        public ICollection<SensorOrbital> Sensores { get; set; } = new List<SensorOrbital>();
    }

    [Table("TB_TIPO_SENSOR")]
    public class TipoSensor
    {
        [Key]
        [Column("ID_TIPO_SENSOR")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdTipoSensor { get; set; }

        [Required]
        [MaxLength(80)]
        [Column("NM_TIPO")]
        public string NmTipo { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        [Column("DS_UNIDADE_MEDIDA")]
        public string DsUnidadeMedida { get; set; } = null!;

        [Required]
        [Column("NR_VALOR_CRITICO")]
        public decimal NrValorCritico { get; set; }

        public ICollection<SensorOrbital> Sensores { get; set; } = new List<SensorOrbital>();
    }

    [Table("TB_SENSOR_ORBITAL")]
    public class SensorOrbital
    {
        [Key]
        [Column("ID_SENSOR_ORBITAL")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdSensorOrbital { get; set; }

        [Required]
        [Column("ID_SATELITE")]
        public long IdSatelite { get; set; }

        [ForeignKey("IdSatelite")]
        public Satelite Satelite { get; set; } = null!;

        [Required]
        [Column("ID_TIPO_SENSOR")]
        public long IdTipoSensor { get; set; }

        [ForeignKey("IdTipoSensor")]
        public TipoSensor TipoSensor { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [Column("NM_SENSOR")]
        public string NmSensor { get; set; } = null!;

        [Required]
        [MaxLength(1)]
        [Column("FL_ATIVO")]
        public string FlAtivo { get; set; } = "S";

        public ICollection<LeituraSatelite> Leituras { get; set; } = new List<LeituraSatelite>();
    }

    [Table("TB_LEITURA_SATELITE")]
    public class LeituraSatelite
    {
        [Key]
        [Column("ID_LEITURA")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdLeitura { get; set; }

        [Required]
        [Column("ID_SENSOR_ORBITAL")]
        public long IdSensorOrbital { get; set; }

        [ForeignKey("IdSensorOrbital")]
        public SensorOrbital SensorOrbital { get; set; } = null!;

        [Required]
        [Column("ID_FAZENDA")]
        public long IdFazenda { get; set; }

        [ForeignKey("IdFazenda")]
        public Fazenda Fazenda { get; set; } = null!;

        [Column("ID_SETOR")]
        public long? IdSetor { get; set; }

        [ForeignKey("IdSetor")]
        public SetorPlantio? Setor { get; set; }

        [Required]
        [Column("NR_VALOR")]
        public decimal NrValor { get; set; }

        // TypeName="DATE" evita que Oracle EF Core tente usar TIMESTAMP(7)
        [Column("DT_LEITURA", TypeName = "DATE")]
        public DateTime DtLeitura { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(1)]
        [Column("FL_ANOMALIA")]
        public string FlAnomalia { get; set; } = "N";

        public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
    }

    [Table("TB_TIPO_ALERTA")]
    public class TipoAlerta
    {
        [Key]
        [Column("ID_TIPO_ALERTA")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdTipoAlerta { get; set; }

        [Required]
        [MaxLength(80)]
        [Column("NM_TIPO_ALERTA")]
        public string NmTipoAlerta { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        [Column("DS_SEVERIDADE")]
        public string DsSeveridade { get; set; } = null!;

        [Required]
        [MaxLength(1)]
        [Column("FL_REQUER_ACAO")]
        public string FlRequerAcao { get; set; } = "N";

        public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
    }

    [Table("TB_ALERTA")]
    public class Alerta
    {
        [Key]
        [Column("ID_ALERTA")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdAlerta { get; set; }

        [Required]
        [Column("ID_LEITURA")]
        public long IdLeitura { get; set; }

        [ForeignKey("IdLeitura")]
        public LeituraSatelite Leitura { get; set; } = null!;

        [Required]
        [Column("ID_TIPO_ALERTA")]
        public long IdTipoAlerta { get; set; }

        [ForeignKey("IdTipoAlerta")]
        public TipoAlerta TipoAlerta { get; set; } = null!;

        [Required]
        [Column("ID_USUARIO")]
        public long IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } = null!;

        [Required]
        [MaxLength(1)]
        [Column("FL_RESOLVIDO")]
        public string FlResolvido { get; set; } = "N";

        // TypeName="DATE" evita mismatch com Oracle DATE
        [Column("DT_ALERTA", TypeName = "DATE")]
        public DateTime DtAlerta { get; set; } = DateTime.Now;

        public ICollection<AcaoAlerta> Acoes { get; set; } = new List<AcaoAlerta>();
    }

    [Table("TB_ACAO_ALERTA")]
    public class AcaoAlerta
    {
        [Key]
        [Column("ID_ACAO")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdAcao { get; set; }

        [Required]
        [Column("ID_ALERTA")]
        public long IdAlerta { get; set; }

        // NULLABLE: evita conflito de change tracker ao inserir junto com UPDATE do Alerta
        [ForeignKey("IdAlerta")]
        public Alerta? Alerta { get; set; }

        [Required]
        [Column("ID_USUARIO")]
        public long IdUsuario { get; set; }

        // NULLABLE: evita conflito de change tracker ao inserir
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        [Required]
        [MaxLength(500)]
        [Column("DS_ACAO_TOMADA")]
        public string DsAcaoTomada { get; set; } = null!;

        // TypeName="DATE" evita mismatch com Oracle DATE
        [Column("DT_ACAO", TypeName = "DATE")]
        public DateTime DtAcao { get; set; } = DateTime.Now;
    }
}
