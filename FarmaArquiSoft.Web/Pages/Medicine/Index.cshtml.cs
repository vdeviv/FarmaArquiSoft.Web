using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Medicines
{
    public class Index : PageModel
    {
        private readonly MedicineApi _api;
        public Index(MedicineApi api) => _api = api;

        public List<MedicineDTO> Medicines { get; private set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                Medicines = await _api.GetAllAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error cargando lista: {ex.Message}";
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var res = await _api.DeleteAsync(id);
                if (res.IsSuccessStatusCode) TempData["SuccessMessage"] = "Eliminado correctamente.";
                else TempData["ErrorMessage"] = "No se pudo eliminar.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar: {ex.Message}";
            }
            return RedirectToPage();
        }
    }
}