using Microsoft.EntityFrameworkCore;
using TrabalhoCapacitacao.Models;

namespace TrabalhoCapacitacao.Data
{
    public class AppDbContext : DbContext
    {
        // Construtor que permite a configuração do DbContext (ex: connection string)
        // ser injetada a partir da classe Startup.cs ou Program.cs
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets para cada uma das suas entidades
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Vaga> Vagas { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Inscricao> Inscricoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da Entidade Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id); // Define a chave primária
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique(); // Garante que o email seja único
                entity.Property(e => e.Perfil).HasMaxLength(50);
                entity.Property(e => e.Telefone).HasMaxLength(20);
            });

            // Configuração da Entidade Vaga
            modelBuilder.Entity<Vaga>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                // Se você quiser ser explícito sobre o SQL Server usar NEWSEQUENTIALID() (melhor para PK clusterizada):
                // entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
                // Ou NEWID():
                //entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");


                entity.Property(e => e.Titulo).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Descricao).IsRequired();
                entity.Property(e => e.Empresa).HasMaxLength(100);
                entity.Property(e => e.Local).HasMaxLength(100);
                entity.Property(e => e.TipoContrato).HasMaxLength(50);
                entity.Property(e => e.DataPublicacao).IsRequired();
            });

            // Configuração da Entidade Curso
            modelBuilder.Entity<Curso>(entity =>
            {
                entity.HasKey(e => e.Id); // Define a chave primária
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Instituicao).HasMaxLength(100);
                entity.Property(e => e.CargaHoraria).HasMaxLength(50);
                entity.Property(e => e.Modalidade).HasMaxLength(50);
                entity.Property(e => e.DataInicio).IsRequired();
            });

            // Configuração da Entidade Inscricao
            modelBuilder.Entity<Inscricao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UsuarioId).IsRequired();

                // VagaId é Guid? e é uma chave estrangeira para Vaga.Id (que é Guid)
                entity.Property(e => e.VagaId).IsRequired(false);

                entity.Property(e => e.CursoId).IsRequired(false); // Supondo que Curso.Id seja string
                entity.Property(e => e.DataInscricao).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);

                entity.HasOne(i => i.Usuario)
                      .WithMany(u => u.Inscricoes)
                      .HasForeignKey(i => i.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento com Vaga (Vaga.Id é Guid)
                entity.HasOne(i => i.Vaga)
                      .WithMany(v => v.Inscricoes)
                      .HasForeignKey(i => i.VagaId) // Chave estrangeira VagaId (Guid?)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                // Supondo que Curso.Id seja string
                entity.HasOne(i => i.Curso)
                      .WithMany(c => c.Inscricoes)
                      .HasForeignKey(i => i.CursoId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}
