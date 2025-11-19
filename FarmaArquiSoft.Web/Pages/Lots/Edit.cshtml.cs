using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;

namespace FarmaArquiSoft.Web.Pages.Lots
{
    public class EditModel : PageModel
    {
        private readonly LotApi _api;

        [BindProperty]
        public LotDTO Input { get; set; } = new();

        public EditModel(LotApi api)
        {
            _api = api;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "ID inválido.";
                return RedirectToPage("Index");
            }

            var lot = await _api.GetByIdAsync(id);
            if (lot == null)
            {
                TempData["ErrorMessage"] = "Lote no encontrado.";
                return RedirectToPage("Index");
            }

            Input = lot;
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                await _api.UpdateAsync(Input.id, Input);
                TempData["SuccessMessage"] = "Lote actualizado correctamente.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}
