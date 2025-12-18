using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Sales
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _http;

        public IndexModel(IHttpClientFactory http)
        {
            _http = http;
        }

        public List<SaleResponseDTO> Sales { get; set; } = new();

        public async Task OnGetAsync()
        {
            var api = _http.CreateClient("SaleApi");
            Sales = await api.GetFromJsonAsync<List<SaleResponseDTO>>("api/sales")
                    ?? new();
        }

        // ===== VER DETALLE =====
        public async Task<IActionResult> OnGetDetailsAsync(string saleId)
        {
            var api = _http.CreateClient("SaleApi");
            var details = await api.GetFromJsonAsync<List<SaleDetailDTO>>(
                $"api/sales/{saleId}/details");

            return new JsonResult(details);
        }

        // ===== ANULAR (ELIMINACIÓN LÓGICA) =====
        public async Task<IActionResult> OnPostToggleStatusAsync(string saleId)
        {
            var api = _http.CreateClient("SaleApi");
            await api.PutAsync($"api/sales/{saleId}/toggle-status", null);
            return RedirectToPage();
        }
    }
}
