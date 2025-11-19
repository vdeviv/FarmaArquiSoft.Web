using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;

namespace FarmaArquiSoft.Web.Pages.Lots
{
    public class CreateModel : PageModel
    {
        private readonly LotApi _api;

        [BindProperty]
        public LotDTO Input { get; set; } = new();

        public CreateModel(LotApi api)
        {
            _api = api;
        }

        public void OnGet()
        {
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                await _api.CreateAsync(Input);
                TempData["SuccessMessage"] = "Lote creado exitosamente.";
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
