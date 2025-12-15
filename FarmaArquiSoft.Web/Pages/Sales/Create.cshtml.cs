using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FarmaArquiSoft.Web.DTOs;

namespace FarmaArquiSoft.Web.Pages.Sales
{
    public class CreateModel : PageModel
    {
        // ===== Datos automáticos =====
        public DateTime Fecha => DateTime.Now;
        public string Usuario => User.Identity?.Name ?? "usuario";

        // ===== Cliente seleccionado =====
        [BindProperty]
        public string ClientId { get; set; } = string.Empty;

        // ===== Detalle temporal de venta =====
        [BindProperty]
        public List<SaleItemTempDTO> Items { get; set; } = new();

        // ===== Total =====
        public decimal Total => Items.Sum(i => i.SubTotal);

        public void OnGet()
        {
            // Inicialización de la vista
        }

        public IActionResult OnPostRemoveItem(string medId)
        {
            Items.RemoveAll(i => i.MedId == medId);
            return Page();
        }
    }
}
