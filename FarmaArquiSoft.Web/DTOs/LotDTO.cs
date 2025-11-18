namespace FarmaArquiSoft.Web.DTOs
{
    public class LotDTO
    {
        public int id { get; set; }
        public int medicine_id { get; set; }
        public string batch_number { get; set; }
        public DateTime expiration_date { get; set; }
        public int quantity { get; set; }
        public decimal unit_cost { get; set; }
        public bool is_deleted { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }

}
