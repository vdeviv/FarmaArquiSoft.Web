using System.Text.Json.Serialization;

namespace FarmaArquiSoft.Web.DTOs
{
    public class LotDTO
    {
        public int id { get; set; }
        public int medicine_id { get; set; }

        // Propiedad Extra: Para mostrar el nombre del medicamento en la tabla (UI Composition)
        public string? MedicineName { get; set; }

        public string batch_number { get; set; } = string.Empty;
        public DateTime expiration_date { get; set; }
        public int quantity { get; set; }
        public decimal unit_cost { get; set; }
        public bool is_deleted { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}