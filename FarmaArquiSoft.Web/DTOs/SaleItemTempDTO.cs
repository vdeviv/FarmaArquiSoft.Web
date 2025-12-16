namespace FarmaArquiSoft.Web.DTOs
{
    // SOLO para la tabla de la UI
    public class SaleItemTempDTO
    {
        public string MedId { get; set; } = string.Empty;
        public string MedName { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public decimal Price { get; set; }

        //SOLO PARA MOSTRAR EL TOTAL
        public decimal SubTotal => Quantity * Price;
    }
}
