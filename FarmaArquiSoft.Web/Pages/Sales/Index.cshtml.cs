using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Sales
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SaleApi _saleApi;

        public List<SaleResponseDTO> Sales { get; set; } = new();

        public IndexModel(SaleApi saleApi)
        {
            _saleApi = saleApi;
        }

        public async Task OnGetAsync()
        {

            Sales = await _saleApi.GetAllAsync();
        }
    }
}
