namespace FarmaArquiSoft.Web.DTOs
{
    public class CreateSaleRequestDTO
    {
        public string ClientId { get; set; } = string.Empty;
        public List<SaleItemDTO> Items { get; set; } = new();
    }

    public class SaleItemDTO
    {
        public string MedId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
