using System.Text.Json.Serialization;

namespace FarmaArquiSoft.Web.DTOs
{
    public class SaleDetailDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("medicineId")]
        public int MedicineId { get; set; }

        [JsonPropertyName("medicineName")]
        public string MedicineName { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
