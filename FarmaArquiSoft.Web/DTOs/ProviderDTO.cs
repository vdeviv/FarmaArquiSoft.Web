namespace FarmaArquiSoft.Web.DTOs
{
    public class ProviderDTO
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public bool is_deleted { get; set; }
        public int created_by { get; set; }
        public DateTime created_at { get; set; }
        public int updated_by { get; set; }
        public DateTime updated_at { get; set; }
    }

}
