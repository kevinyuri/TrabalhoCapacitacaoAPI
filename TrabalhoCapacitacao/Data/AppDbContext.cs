using Microsoft.EntityFrameworkCore;
using TrabalhoCapacitacao.Models;

namespace TrabalhoCapacitacao.Data
{
    public class AppDbContext : DbContext
    {
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
                entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique(); // Garante que o email seja único
                entity.Property(e => e.Perfil).HasMaxLength(50);
                entity.Property(e => e.Telefone).HasMaxLength(20);
                entity.Property(e => e.SenhaHash).IsRequired();


            });

            // Configuração da Entidade Vaga
            modelBuilder.Entity<Vaga>(entity =>
            {
                entity.HasKey(e => e.Id); // Define a chave primária
                entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
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
                entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Instituicao).HasMaxLength(100);
                entity.Property(e => e.CargaHoraria).HasMaxLength(50);
                entity.Property(e => e.Modalidade).HasMaxLength(50);
                entity.Property(e => e.DataInicio).IsRequired();
            });

            // Configuração da Entidade Inscricao
            modelBuilder.Entity<Inscricao>(entity =>
            {
                entity.HasKey(e => e.Id); // Define a chave primária
                entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.UsuarioId).IsRequired();
                entity.Property(e => e.VagaId).IsRequired(false); // Permite nulo
                entity.Property(e => e.CursoId).IsRequired(false); // Permite nulo
                entity.Property(e => e.DataInscricao).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);

                // Relacionamento: Uma Inscrição pertence a um Usuário
                entity.HasOne(i => i.Usuario)
                      .WithMany(u => u.Inscricoes)
                      .HasForeignKey(i => i.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento: Uma Inscrição pode pertencer a uma Vaga (opcional)
                entity.HasOne(i => i.Vaga)
                      .WithMany(v => v.Inscricoes) // Uma vaga pode ter muitas inscrições
                      .HasForeignKey(i => i.VagaId)
                      .IsRequired(false) // A chave estrangeira é opcional
                      .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento: Uma Inscrição pode pertencer a um Curso (opcional)
                entity.HasOne(i => i.Curso)
                      .WithMany(c => c.Inscricoes) // Um curso pode ter muitas inscrições
                      .HasForeignKey(i => i.CursoId)
                      .IsRequired(false) // A chave estrangeira é opcional
                      .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }

}
