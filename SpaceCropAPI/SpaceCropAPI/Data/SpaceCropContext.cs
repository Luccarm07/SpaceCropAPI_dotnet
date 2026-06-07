using Microsoft.EntityFrameworkCore;
using SpaceCropAPI.Models;

namespace SpaceCropAPI.Data
{
    public class SpaceCropContext : DbContext
    {
        public SpaceCropContext(DbContextOptions<SpaceCropContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Fazenda> Fazendas { get; set; }
        public DbSet<SetorPlantio> SetoresPlantio { get; set; }
        public DbSet<Satelite> Satelites { get; set; }
        public DbSet<TipoSensor> TiposSensor { get; set; }
        public DbSet<SensorOrbital> SensoresOrbitais { get; set; }
        public DbSet<LeituraSatelite> LeiturasSatelite { get; set; }
        public DbSet<TipoAlerta> TiposAlerta { get; set; }
        public DbSet<Alerta> Alertas { get; set; }
        public DbSet<AcaoAlerta> AcoesAlerta { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Precisão decimal para Oracle
            modelBuilder.Entity<Fazenda>()
                .Property(f => f.NrAreaHectares)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SetorPlantio>()
                .Property(s => s.NrAreaHectares)
                .HasPrecision(10, 2);

            modelBuilder.Entity<TipoSensor>()
                .Property(t => t.NrValorCritico)
                .HasPrecision(8, 2);

            modelBuilder.Entity<LeituraSatelite>()
                .Property(l => l.NrValor)
                .HasPrecision(8, 2);

            // Índices únicos
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.DsEmail)
                .IsUnique();

            // Relacionamentos sem cascade delete automático (Oracle não suporta bem)
            modelBuilder.Entity<Fazenda>()
                .HasOne(f => f.Usuario)
                .WithMany(u => u.Fazendas)
                .HasForeignKey(f => f.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SetorPlantio>()
                .HasOne(s => s.Fazenda)
                .WithMany(f => f.SetoresPlantio)
                .HasForeignKey(s => s.IdFazenda)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SensorOrbital>()
                .HasOne(s => s.Satelite)
                .WithMany(sat => sat.Sensores)
                .HasForeignKey(s => s.IdSatelite)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SensorOrbital>()
                .HasOne(s => s.TipoSensor)
                .WithMany(t => t.Sensores)
                .HasForeignKey(s => s.IdTipoSensor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeituraSatelite>()
                .HasOne(l => l.SensorOrbital)
                .WithMany(s => s.Leituras)
                .HasForeignKey(l => l.IdSensorOrbital)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeituraSatelite>()
                .HasOne(l => l.Fazenda)
                .WithMany(f => f.Leituras)
                .HasForeignKey(l => l.IdFazenda)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeituraSatelite>()
                .HasOne(l => l.Setor)
                .WithMany(s => s.Leituras)
                .HasForeignKey(l => l.IdSetor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Alerta>()
                .HasOne(a => a.Leitura)
                .WithMany(l => l.Alertas)
                .HasForeignKey(a => a.IdLeitura)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Alerta>()
                .HasOne(a => a.TipoAlerta)
                .WithMany(t => t.Alertas)
                .HasForeignKey(a => a.IdTipoAlerta)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Alerta>()
                .HasOne(a => a.Usuario)
                .WithMany(u => u.Alertas)
                .HasForeignKey(a => a.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AcaoAlerta>()
                .HasOne(ac => ac.Alerta)
                .WithMany(a => a.Acoes)
                .HasForeignKey(ac => ac.IdAlerta)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AcaoAlerta>()
                .HasOne(ac => ac.Usuario)
                .WithMany(u => u.AcoesAlerta)
                .HasForeignKey(ac => ac.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
