namespace TrabalhoCapacitacao.Models
{
    public class Inscricao
    {
        public string Id { get; set; } // PK
        public string UsuarioId { get; set; } // FK
        public string? VagaId { get; set; } // FK, pode ser nulo se for inscrição em curso
        public string? CursoId { get; set; } // FK, pode ser nulo se for inscrição em vaga
        public DateTime DataInscricao { get; set; }
        public string Status { get; set; }

        // Propriedades de Navegação
        public virtual Usuario Usuario { get; set; }
        public virtual Vaga Vaga { get; set; }
        public virtual Curso Curso { get; set; }
    }
}
