using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; // Importante para JsonResult
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Sales
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SaleApi _saleApi;
        private readonly SaleDetailApi _saleDetailApi; // <--- Inyectamos el nuevo servicio

        public List<SaleResponseDTO> Sales { get; set; } = new();

        public IndexModel(SaleApi saleApi, SaleDetailApi saleDetailApi)
        {
            _saleApi = saleApi;
            _saleDetailApi = saleDetailApi;
        }

        public async Task OnGetAsync()
        {
            Sales = await _saleApi.GetAllAsync();
        }

        // --- NUEVO HANDLER PARA AJAX ---
        public async Task<JsonResult> OnGetSaleDetailsAsync(string id)
        {
            Console.WriteLine($" RAZOR HANDLER: Solicitud recibida para ID: {id}");
            var details = await _saleDetailApi.GetBySaleIdAsync(id);
            return new JsonResult(details);
        }
    }
}