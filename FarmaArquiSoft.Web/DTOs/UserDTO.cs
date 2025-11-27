using System.Text.Json.Serialization;

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
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("username")]
        public string username { get; set; } = "";

        [JsonPropertyName("firstName")]
        public string first_name { get; set; } = "";

        [JsonPropertyName("lastFirstName")]
        public string last_first_name { get; set; } = "";

        [JsonPropertyName("lastSecondName")]
        public string last_second_name { get; set; } = "";

        [JsonPropertyName("mail")]      
        public string? mail { get; set; }

        [JsonPropertyName("phone")]
        public string phone { get; set; } = "";

        [JsonPropertyName("ci")]
        public string ci { get; set; } = "";

        [JsonPropertyName("role")]
        public UserRole role { get; set; }

        [JsonPropertyName("has_changed_password")]
        public bool has_changed_password { get; set; } = true;

        [JsonPropertyName("password_version")]
        public int password_version { get; set; } = 0;
        public bool IsActive { get; set; }
    }

    public class UserListItemDto
    {
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("username")]
        public string username { get; set; } = "";

        [JsonPropertyName("lastFirstName")]
        public string last_first_name { get; set; } = "";

        [JsonPropertyName("lastSecondName")]
        public string last_second_name { get; set; } = "";

        [JsonPropertyName("mail")]
        public string? mail { get; set; }

        [JsonPropertyName("phone")]
        public string phone { get; set; } = "";

        [JsonPropertyName("ci")]
        public string ci { get; set; } = "";

        [JsonPropertyName("role")]
        public UserRole role { get; set; }
    }
}
