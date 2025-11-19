namespace FarmaArquiSoft.Web.DTOs
{
    public enum UserRole
    {
        Administrador,
        Cajero,
        Almacenero
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastFirstName { get; set; } = "";
        public string LastSecondName { get; set; } = "";
        public string? Mail { get; set; }
        public string Phone { get; set; } = "";
        public string Ci { get; set; } = "";
        public UserRole Role { get; set; }

        // Opcional: no existía en la API de lectura, lo comentamos
        public bool IsActive { get; set; }
    }
    public class UserListItemDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string LastFirstName { get; set; } = "";
        public string? LastSecondName { get; set; }
        public string? Mail { get; set; }
        public string Phone { get; set; } = "";
        public string Ci { get; set; } = "";
        public UserRole Role { get; set; }
    }
}
