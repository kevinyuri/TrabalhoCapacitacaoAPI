using System.ComponentModel.DataAnnotations;

namespace TrabalhoCapacitacao.DTOs.Usuario
{
    public class UsuarioCreateDto
    {
        [Required(ErrorMessage = "O nome do usuário é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O e-mail do usuário é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string Senha { get; set; } // Considere nomear como PasswordConfirm se houver confirmação

        [Required(ErrorMessage = "O perfil do usuário é obrigatório.")]
        [StringLength(50, ErrorMessage = "O perfil deve ter no máximo 50 caracteres.")]
        public string Perfil { get; set; } // Ex: "candidato", "empresa", "admin"

        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
        public string? Telefone { get; set; }
    }
}
