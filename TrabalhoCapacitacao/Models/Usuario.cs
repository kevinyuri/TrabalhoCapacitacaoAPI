namespace TrabalhoCapacitacao.Models
{
    public class Usuario
    {
        public string Id { get; set; } // PK
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Perfil { get; set; } // Ex: "candidato", "empresa", "admin"
        public string Telefone { get; set; }
        public string SenhaHash { get; set; }

        // Relacionamento: Um usuário pode ter várias inscrições
        public virtual ICollection<Inscricao> Inscricoes { get; set; }

        public Usuario()
        {
            Inscricoes = new HashSet<Inscricao>();
        }
    }
}
