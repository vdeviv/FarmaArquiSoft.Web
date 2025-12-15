namespace FarmaArquiSoft.Web.DTOs
{
    public class SaleItemTempDTO
    {
        public string MedId { get; set; } = string.Empty;
        public string MedName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public decimal SubTotal => Quantity * Price;
    }
}
