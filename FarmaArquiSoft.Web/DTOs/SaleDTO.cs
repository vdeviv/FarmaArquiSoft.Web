using System.Text.Json.Serialization;

namespace FarmaArquiSoft.Web.DTOs
{
    // DTO para enviar la venta al API (POST)
    public class CreateSaleRequest
    {
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<SaleItemPayload> Items { get; set; } = new();
    }

    public class SaleItemPayload
    {
        [JsonPropertyName("medId")]
        public string MedId { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonIgnore]
        public string? MedName { get; set; }

        [JsonIgnore]
        public decimal SubTotal => Price * Quantity;
    }
    public class SaleResponseDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }

        public string? ClientName { get; set; }
    }
}