using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FarmaArquiSoft.Web.Pages.Medicines
{
    public class Create : PageModel
    {
        private readonly MedicineApi _api;
        private readonly ProviderApi _pApi;

        public Create(MedicineApi api, ProviderApi pApi) { _api = api; _pApi = pApi; }

        [BindProperty] public MedicineDTO Medicine { get; set; } = new();
        public List<SelectListItem> ProviderOptions { get; set; } = new();

        public async Task OnGetAsync() => await LoadDeps();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { await LoadDeps(); return Page(); }
            var res = await _api.CreateAsync(Medicine);
            if (res.IsSuccessStatusCode) return RedirectToPage("Index");
            ModelState.AddModelError("", "Error al crear.");
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