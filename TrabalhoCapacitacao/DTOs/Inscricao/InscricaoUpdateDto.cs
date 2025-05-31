using System.ComponentModel.DataAnnotations;

namespace TrabalhoCapacitacao.DTOs.Inscricao
{
    public class InscricaoUpdateDto
    {
        [Required(ErrorMessage = "O status da inscrição é obrigatório.")]
        [StringLength(50, ErrorMessage = "O status deve ter no máximo 50 caracteres.")]
        public string Status { get; set; }
    }
}
