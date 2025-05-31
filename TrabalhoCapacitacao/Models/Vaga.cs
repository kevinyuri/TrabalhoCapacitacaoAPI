using System.ComponentModel.DataAnnotations.Schema;

namespace TrabalhoCapacitacao.Models
{
    public class Vaga
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } // PK
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string Empresa { get; set; }
        public string Local { get; set; }
        public string TipoContrato { get; set; }
        public DateTime DataPublicacao { get; set; }

        // Relacionamento: Uma vaga pode ter várias inscrições
        public virtual ICollection<Inscricao> Inscricoes { get; set; }

        public Vaga()
        {
            Inscricoes = new HashSet<Inscricao>();
        }
    }
}
