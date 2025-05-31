using System.ComponentModel.DataAnnotations;

namespace TrabalhoCapacitacao.DTOs.Vaga
{
    public class VagaUpdateDto
    {
        [Required(ErrorMessage = "O título da vaga é obrigatório.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "O título deve ter entre 3 e 150 caracteres.")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "A descrição da vaga é obrigatória.")]
        public string Descricao { get; set; }

        [StringLength(100, ErrorMessage = "O nome da empresa deve ter no máximo 100 caracteres.")]
        public string? Empresa { get; set; }

        [StringLength(100, ErrorMessage = "O local deve ter no máximo 100 caracteres.")]
        public string? Local { get; set; }

        [StringLength(50, ErrorMessage = "O tipo de contrato deve ter no máximo 50 caracteres.")]
        public string? TipoContrato { get; set; }

        // DataPublicacao não costuma ser atualizada pelo usuário, mas sim pelo sistema.
        // Se precisar permitir, adicione aqui.
    }
}
