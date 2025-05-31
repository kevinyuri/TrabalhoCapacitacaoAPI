namespace TrabalhoCapacitacao.DTOs.Inscricao
{
    public class InscricaoResponseDto
    {
        public string Id { get; set; }
        public DateTime DataInscricao { get; set; }
        public string Status { get; set; }
        public string UsuarioId { get; set; }
        public string? NomeUsuario { get; set; }
        public string? VagaId { get; set; }
        public string? TituloVaga { get; set; }
        public string? CursoId { get; set; }
        public string? NomeCurso { get; set; }
    }
}
