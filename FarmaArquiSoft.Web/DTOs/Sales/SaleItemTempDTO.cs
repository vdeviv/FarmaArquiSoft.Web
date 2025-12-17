namespace FarmaArquiSoft.Web.DTOs
{
    // SOLO para la tabla de la UI
        public class SaleItemTempDTO
    {
        public int LotId { get; set; }
        public int MedId { get; set; }
        public string MedName { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public decimal SubTotal => Quantity * Price;
    }

}
