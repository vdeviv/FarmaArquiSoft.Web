using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FarmaArquiSoft.Web.DTOs;        
using FarmaArquiSoft.Web.Services;    

namespace FarmaArquiSoft.Web.Pages.Lots
{
    public class IndexModel : PageModel
    {
        private readonly LotApi _lotApi;

        public List<LotDTO> Lots { get; private set; } = new();

        public IndexModel(LotApi lotApi)
        {
            _lotApi = lotApi;
        }

        public async Task OnGetAsync()
        {
            Lots = await _lotApi.GetAllAsync();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "ID de lote inválido.";
                return RedirectToPage();
            }

            try
            {
                await _lotApi.DeleteAsync(id);
                TempData["SuccessMessage"] = "Lote eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"No se pudo eliminar el lote: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
