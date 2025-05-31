using System.ComponentModel.DataAnnotations;

namespace TrabalhoCapacitacao.DTOs.Inscricao
{
    public class InscricaoCreateDto
    {
        [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
        public string UsuarioId { get; set; }
        public string? VagaId { get; set; }
        public string? CursoId { get; set; }
        [Required(ErrorMessage = "O status da inscrição é obrigatório.")]
        [StringLength(50, ErrorMessage = "O status deve ter no máximo 50 caracteres.")]
        public string Status { get; set; } // Ex: "Pendente", "Confirmada", "Cancelada"
    }
}
