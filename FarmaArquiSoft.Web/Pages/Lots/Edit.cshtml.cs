using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FarmaArquiSoft.Web.Pages.Lots
{
    public class EditModel : PageModel
    {
        private readonly LotApi _api;
        private readonly MedicineApi _mApi;

        public EditModel(LotApi api, MedicineApi mApi) { _api = api; _mApi = mApi; }

        [BindProperty] public LotDTO Input { get; set; } = new();
        public List<SelectListItem> MedicineOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var l = await _api.GetByIdAsync(id);
            if (l == null) return RedirectToPage("Index");
            Input = l;
            await LoadDeps();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { await LoadDeps(); return Page(); }
            var res = await _api.UpdateAsync(Input);
            if (res.IsSuccessStatusCode) return RedirectToPage("Index");
            await LoadDeps();
            return Page();
        }

        private async Task LoadDeps()
        {
            var meds = await _mApi.GetAllAsync();
            MedicineOptions = meds.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = $"{m.Name} ({m.Presentation})" }).ToList();
        }
    }
}