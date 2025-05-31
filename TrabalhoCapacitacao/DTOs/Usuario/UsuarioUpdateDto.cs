using System.ComponentModel.DataAnnotations;

namespace TrabalhoCapacitacao.DTOs.Usuario
{
    public class UsuarioUpdateDto
    {
        [Required(ErrorMessage = "O nome do usuário é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        public string Nome { get; set; }

        // O e-mail geralmente não é alterado ou requer um processo de verificação.
        // Se permitir alteração, adicione validações.
        // public string Email { get; set; }

        [Required(ErrorMessage = "O perfil do usuário é obrigatório.")]
        [StringLength(50, ErrorMessage = "O perfil deve ter no máximo 50 caracteres.")]
        public string Perfil { get; set; }

        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
        public string? Telefone { get; set; }
    }
}
