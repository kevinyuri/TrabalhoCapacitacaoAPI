using System;
using System.ComponentModel.DataAnnotations;

namespace TrabalhoCapacitacao.DTOs.Curso
{
    public class CursoUpdateDto
    {
        [Required(ErrorMessage = "O nome do curso é obrigatório.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome do curso deve ter entre 3 e 150 caracteres.")]
        public string Nome { get; set; }

        [StringLength(100, ErrorMessage = "O nome da instituição deve ter no máximo 100 caracteres.")]
        public string? Instituicao { get; set; }

        [StringLength(50, ErrorMessage = "A carga horária deve ter no máximo 50 caracteres.")]
        public string? CargaHoraria { get; set; }

        [StringLength(50, ErrorMessage = "A modalidade deve ter no máximo 50 caracteres.")]
        public string? Modalidade { get; set; }

        [Required(ErrorMessage = "A data de início do curso é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime DataInicio { get; set; }
    }

}
