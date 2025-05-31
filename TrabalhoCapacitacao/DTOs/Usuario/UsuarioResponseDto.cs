namespace TrabalhoCapacitacao.DTOs.Usuario
{
    public class UsuarioResponseDto
    {
        public string Id { get; set; } // ID do usuário (pode ser do Keycloak)
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Perfil { get; set; }
        public string? Telefone { get; set; }
    }
}
