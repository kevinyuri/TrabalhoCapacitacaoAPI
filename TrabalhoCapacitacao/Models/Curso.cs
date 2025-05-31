namespace TrabalhoCapacitacao.Models
{
    public class Curso
    {
        public string Id { get; set; } // PK
        public string Nome { get; set; }
        public string Instituicao { get; set; }
        public string CargaHoraria { get; set; }
        public string Modalidade { get; set; }
        public DateTime DataInicio { get; set; }

        // Relacionamento: Um curso pode ter várias inscrições
        public virtual ICollection<Inscricao> Inscricoes { get; set; }

        public Curso()
        {
            Inscricoes = new HashSet<Inscricao>();
        }
    }
}
