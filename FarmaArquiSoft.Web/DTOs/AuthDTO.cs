using System;
namespace FarmaArquiSoft.Web.DTOs
{
    // Petición de autenticación enviada al API
    public class AuthenticateRequestDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Respuesta del endpoint /authenticate
    public class AuthenticateResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public UserListItemDto? User { get; set; }
    }

    // Petición para cambiar contraseña
    public class ChangePasswordRequestDTO
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}