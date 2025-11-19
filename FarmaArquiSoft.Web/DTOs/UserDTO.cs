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

        // 👇 Estos 3 son los críticos, aquí estaba el problema
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
