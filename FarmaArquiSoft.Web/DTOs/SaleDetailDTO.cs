using System.Text.Json.Serialization;

namespace FarmaArquiSoft.Web.DTOs
{
    public class SaleDetailDTO // Asegurate que el nombre coincida (DTO vs Dto)
    {
        // Acepta "id" o "Id"
        [JsonPropertyName("id")]
        public int Id { get; set; }

        // Acepta medicineId (camel), MedicineId (Pascal) o medicine_id (snake)
        [JsonPropertyName("medicineId")]
        public int MedicineId { get; set; }

        // Propiedad extra para mapear snake_case si el serializador falla
        [JsonPropertyName("medicine_id")]
        public int MedicineId_Snake { set { MedicineId = value; } }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal UnitPrice_Snake { set { UnitPrice = value; } }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("total_amount")]
        public decimal TotalAmount_Snake { set { TotalAmount = value; } }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}