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

        // Coincide con tu JSON: "firstName"
        [JsonPropertyName("firstName")]
        public string first_name { get; set; } = "";

        // Coincide con tu JSON: "lastFirstName"
        [JsonPropertyName("lastFirstName")]
        public string last_first_name { get; set; } = "";

        // Coincide con tu JSON: "lastSecondName"
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

        [JsonPropertyName("hasChangedPassword")]
        public bool has_changed_password { get; set; }

        [JsonPropertyName("passwordVersion")]
        public int password_version { get; set; } = 0;

        [JsonPropertyName("lastPasswordChangedAt")]
        public DateTime? last_password_changed_at { get; set; }

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