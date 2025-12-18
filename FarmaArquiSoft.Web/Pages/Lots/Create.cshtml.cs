using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FarmaArquiSoft.Web.Pages.Lots
{
    public class CreateModel : PageModel
    {
        private readonly LotApi _api;
        private readonly MedicineApi _mApi;

        public CreateModel(LotApi api, MedicineApi mApi) { _api = api; _mApi = mApi; }

        [BindProperty] public LotDTO Input { get; set; } = new();
        public List<SelectListItem> MedicineOptions { get; set; } = new();

        public async Task OnGetAsync()
        {
            Input.expiration_date = DateTime.Today.AddMonths(6);
            await LoadDeps();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { await LoadDeps(); return Page(); }
            var res = await _api.CreateAsync(Input);
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