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

        // ===== Buscadores =====
        [BindProperty]
        public string ClientSearch { get; set; } = string.Empty;

        [BindProperty]
        public string ClientName { get; set; } = string.Empty;

        [BindProperty]
        public string ProductSearch { get; set; } = string.Empty;

        // ===== Detalle =====
        [BindProperty]
        public List<SaleItemTempDTO> Items { get; set; } = new();

        public decimal Total => Items.Sum(i => i.SubTotal);

        public void OnGet()
        {
            // Inicialización
        }

        public IActionResult OnPostRemoveItem(string medId)
        {
            Items.RemoveAll(i => i.MedId == medId);
            return Page();
        }
    }
}
