namespace TrabalhoCapacitacao.DTOs.Curso
{
    public class CursoResponseDto
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string? Instituicao { get; set; }
        public string? CargaHoraria { get; set; }
        public string? Modalidade { get; set; }
        public DateTime DataInicio { get; set; }
    }
}
