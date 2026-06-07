using AutoMapper;
using SpaceCropAPI.Models;
using SpaceCropAPI.DTOs;

namespace SpaceCropAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Usuario
            CreateMap<Usuario, UsuarioResponseDTO>()
                .ForMember(d => d.Id,    o => o.MapFrom(s => s.IdUsuario))
                .ForMember(d => d.Nome,  o => o.MapFrom(s => s.NmUsuario))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.DsEmail));

            CreateMap<UsuarioRequestDTO, Usuario>()
                .ForMember(d => d.NmUsuario,   o => o.MapFrom(s => s.Nome))
                .ForMember(d => d.DsEmail,     o => o.MapFrom(s => s.Email))
                .ForMember(d => d.DsSenhaHash, o => o.MapFrom(s => s.Senha)); // hash feito no controller

            // Fazenda
            CreateMap<Fazenda, FazendaResponseDTO>()
                .ForMember(d => d.Id,           o => o.MapFrom(s => s.IdFazenda))
                .ForMember(d => d.UsuarioId,    o => o.MapFrom(s => s.IdUsuario))
                .ForMember(d => d.Nome,         o => o.MapFrom(s => s.NmFazenda))
                .ForMember(d => d.Cidade,       o => o.MapFrom(s => s.DsCidade))
                .ForMember(d => d.Estado,       o => o.MapFrom(s => s.DsEstado))
                .ForMember(d => d.AreaHectares, o => o.MapFrom(s => s.NrAreaHectares));

            CreateMap<FazendaRequestDTO, Fazenda>()
                .ForMember(d => d.NmFazenda,       o => o.MapFrom(s => s.Nome))
                .ForMember(d => d.DsCidade,        o => o.MapFrom(s => s.Cidade))
                .ForMember(d => d.DsEstado,        o => o.MapFrom(s => s.Estado))
                .ForMember(d => d.NrAreaHectares,  o => o.MapFrom(s => s.AreaHectares))
                .ForMember(d => d.IdUsuario,       o => o.MapFrom(s => s.UsuarioId));

            // SetorPlantio
            CreateMap<SetorPlantio, SetorResponseDTO>()
                .ForMember(d => d.Id,           o => o.MapFrom(s => s.IdSetor))
                .ForMember(d => d.FazendaId,    o => o.MapFrom(s => s.IdFazenda))
                .ForMember(d => d.Nome,         o => o.MapFrom(s => s.NmSetor))
                .ForMember(d => d.Cultura,      o => o.MapFrom(s => s.DsCultura))
                .ForMember(d => d.AreaHectares, o => o.MapFrom(s => s.NrAreaHectares));

            CreateMap<SetorRequestDTO, SetorPlantio>()
                .ForMember(d => d.NmSetor,        o => o.MapFrom(s => s.Nome))
                .ForMember(d => d.DsCultura,      o => o.MapFrom(s => s.Cultura))
                .ForMember(d => d.NrAreaHectares, o => o.MapFrom(s => s.AreaHectares));

            // Satelite
            CreateMap<Satelite, SateliteResponseDTO>()
                .ForMember(d => d.Id,       o => o.MapFrom(s => s.IdSatelite))
                .ForMember(d => d.Nome,     o => o.MapFrom(s => s.NmSatelite))
                .ForMember(d => d.Operador, o => o.MapFrom(s => s.DsOperador))
                .ForMember(d => d.Ativo,    o => o.MapFrom(s => s.FlAtivo == "S"));

            // SensorOrbital
            CreateMap<SensorOrbital, SensorOrbitalResponseDTO>()
                .ForMember(d => d.Id,            o => o.MapFrom(s => s.IdSensorOrbital))
                .ForMember(d => d.SateliteId,    o => o.MapFrom(s => s.IdSatelite))
                .ForMember(d => d.TipoSensorId,  o => o.MapFrom(s => s.IdTipoSensor))
                .ForMember(d => d.Nome,          o => o.MapFrom(s => s.NmSensor))
                .ForMember(d => d.Ativo,         o => o.MapFrom(s => s.FlAtivo == "S"))
                .ForMember(d => d.TipoNome,      o => o.MapFrom(s => s.TipoSensor != null ? s.TipoSensor.NmTipo : null))
                .ForMember(d => d.UnidadeMedida, o => o.MapFrom(s => s.TipoSensor != null ? s.TipoSensor.DsUnidadeMedida : null))
                .ForMember(d => d.ValorCritico,  o => o.MapFrom(s => s.TipoSensor != null ? (decimal?)s.TipoSensor.NrValorCritico : null));

            // LeituraSatelite
            CreateMap<LeituraSatelite, LeituraResponseDTO>()
                .ForMember(d => d.Id,              o => o.MapFrom(s => s.IdLeitura))
                .ForMember(d => d.SensorOrbitalId, o => o.MapFrom(s => s.IdSensorOrbital))
                .ForMember(d => d.FazendaId,       o => o.MapFrom(s => s.IdFazenda))
                .ForMember(d => d.SetorId,         o => o.MapFrom(s => s.IdSetor))
                .ForMember(d => d.Valor,           o => o.MapFrom(s => s.NrValor))
                .ForMember(d => d.DataLeitura,     o => o.MapFrom(s => s.DtLeitura))
                .ForMember(d => d.Anomalia,        o => o.MapFrom(s => s.FlAnomalia == "S"))
                .ForMember(d => d.SensorNome,      o => o.MapFrom(s => s.SensorOrbital != null ? s.SensorOrbital.NmSensor : null))
                .ForMember(d => d.UnidadeMedida,   o => o.MapFrom(s => s.SensorOrbital != null && s.SensorOrbital.TipoSensor != null ? s.SensorOrbital.TipoSensor.DsUnidadeMedida : null));

            CreateMap<LeituraRequestDTO, LeituraSatelite>()
                .ForMember(d => d.IdSensorOrbital, o => o.MapFrom(s => s.SensorOrbitalId))
                .ForMember(d => d.IdFazenda,       o => o.MapFrom(s => s.FazendaId))
                .ForMember(d => d.IdSetor,         o => o.MapFrom(s => s.SetorId))
                .ForMember(d => d.NrValor,         o => o.MapFrom(s => s.Valor))
                .ForMember(d => d.DtLeitura,       o => o.MapFrom(s => s.DataLeitura ?? DateTime.Now))
                .ForMember(d => d.FlAnomalia,      o => o.MapFrom(s => s.Anomalia ? "S" : "N"));

            // Alerta
            CreateMap<Alerta, AlertaResponseDTO>()
                .ForMember(d => d.Id,           o => o.MapFrom(s => s.IdAlerta))
                .ForMember(d => d.LeituraId,    o => o.MapFrom(s => s.IdLeitura))
                .ForMember(d => d.TipoAlertaId, o => o.MapFrom(s => s.IdTipoAlerta))
                .ForMember(d => d.UsuarioId,    o => o.MapFrom(s => s.IdUsuario))
                .ForMember(d => d.Resolvido,    o => o.MapFrom(s => s.FlResolvido == "S"))
                .ForMember(d => d.DataAlerta,   o => o.MapFrom(s => s.DtAlerta))
                .ForMember(d => d.TipoNome,     o => o.MapFrom(s => s.TipoAlerta != null ? s.TipoAlerta.NmTipoAlerta : null))
                .ForMember(d => d.Severidade,   o => o.MapFrom(s => s.TipoAlerta != null ? s.TipoAlerta.DsSeveridade : null));

            CreateMap<AlertaRequestDTO, Alerta>()
                .ForMember(d => d.IdLeitura,    o => o.MapFrom(s => s.LeituraId))
                .ForMember(d => d.IdTipoAlerta, o => o.MapFrom(s => s.TipoAlertaId))
                .ForMember(d => d.IdUsuario,    o => o.MapFrom(s => s.UsuarioId));
        }
    }
}
