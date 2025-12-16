namespace FarmaArquiSoft.Web.DTOs
{
    public class CreateSaleRequestDto
    {
        public string ClientId { get; set; } = string.Empty;
        public List<SaleItemDto> Items { get; set; } = new();
    }

    public class SaleItemDto
    {
        public string MedId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
