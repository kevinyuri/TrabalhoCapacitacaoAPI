using System;
using System.ComponentModel.DataAnnotations;


public class CursoCreateDto
{
    [Required(ErrorMessage = "O nome do curso é obrigatório.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome do curso deve ter entre 3 e 150 caracteres.")]
    public string Nome { get; set; }

    [StringLength(100, ErrorMessage = "O nome da instituição deve ter no máximo 100 caracteres.")]
    public string? Instituicao { get; set; }

    [StringLength(50, ErrorMessage = "A carga horária deve ter no máximo 50 caracteres.")]
    public string? CargaHoraria { get; set; } // Ex: "40 horas", "3 meses"

    [StringLength(50, ErrorMessage = "A modalidade deve ter no máximo 50 caracteres.")]
    public string? Modalidade { get; set; } // Ex: "Online", "Presencial", "Híbrido"

    [Required(ErrorMessage = "A data de início do curso é obrigatória.")]
    [DataType(DataType.Date)] // Apenas data, sem hora, se apropriado
    public DateTime DataInicio { get; set; }
}
