using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FarmaArquiSoft.Web.Pages.Medicines
{
    public class Edit : PageModel
    {
        private readonly MedicineApi _api;
        private readonly ProviderApi _pApi;

        public Edit(MedicineApi api, ProviderApi pApi) { _api = api; _pApi = pApi; }

        [BindProperty] public MedicineDTO Medicine { get; set; } = new();
        public List<SelectListItem> ProviderOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var m = await _api.GetByIdAsync(id);
            if (m == null) return RedirectToPage("Index");
            Medicine = m;
            await LoadDeps();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { await LoadDeps(); return Page(); }
            var res = await _api.UpdateAsync(Medicine);
            if (res.IsSuccessStatusCode) return RedirectToPage("Index");
            ModelState.AddModelError("", "Error al actualizar.");
            await LoadDeps();
            return Page();
        }

        private async Task LoadDeps()
        {
            var pros = await _pApi.GetAllAsync();
            ProviderOptions = pros.Select(p => new SelectListItem { Value = p.id.ToString(), Text = $"{p.first_name} {p.last_name}" }).ToList();
        }
    }
}