using System.Text.Json.Serialization;

namespace FarmaArquiSoft.Web.DTOs
{
    public class MedicineDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("presentation")]
        public MedicinePresentation Presentation { get; set; }

        [JsonPropertyName("provider_id")]
        public int ProviderId { get; set; }

        public string? ProviderName { get; set; }

        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Relación con Lotes
        [JsonPropertyName("linkedLots")] // El backend lo envía como 'linkedLots' o 'LinkedLots'
        public List<MedicineLotLinkDTO> LinkedLots { get; set; } = new();
    }

    public class MedicineLotLinkDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("medicine_id")]
        public int MedicineId { get; set; }

        [JsonPropertyName("lot_id")]
        public int LotId { get; set; }

        // Propiedad extra para MOSTRAR el número de lote en el Front (opcional)
        public string? BatchNumber { get; set; }
    }
}