using System.Text.Json.Serialization;

namespace FarmaArquiSoft.Web.DTOs
{
    public class MedicineDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        // Aquí usamos el Enum que acabamos de crear
        [JsonPropertyName("presentation")]
        public MedicinePresentation Presentation { get; set; }

        [JsonPropertyName("provider_id")]
        public int ProviderId { get; set; }

        // Propiedad Extra: Para mostrar el nombre del proveedor en la tabla (UI Composition)
        public string? ProviderName { get; set; }

        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}