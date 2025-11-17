using System.ComponentModel.DataAnnotations;

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
        public string CiNit { get; set; }
        public string RazonSocial { get; set; }

        public string FirstName { get; set; }
        public string LastFirstName { get; set; }
        public string LastSecondName { get; set; }
        public string Mail { get; set; }
        public string Phone { get; set; }
        public string Ci { get; set; }
        public UserRole Role { get; set; }

        public bool IsActive { get; set; }
        public string? Username { get; internal set; }
    }
}