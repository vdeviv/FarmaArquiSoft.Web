using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Medicines
{
    public class Index : PageModel
    {
        private readonly MedicineApi _medicineApi;
        private readonly ProviderApi _providerApi;

        public Index(MedicineApi medicineApi, ProviderApi providerApi)
        {
            _medicineApi = medicineApi;
            _providerApi = providerApi;
        }

        public List<MedicineDTO> Medicines { get; private set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var tMeds = _medicineApi.GetAllAsync();
                var tProvs = _providerApi.GetAllAsync();
                await Task.WhenAll(tMeds, tProvs);

                Medicines = await tMeds;
                var providers = await tProvs;
                var pDict = providers.ToDictionary(p => p.id, p => $"{p.first_name} {p.last_name}");

                foreach (var m in Medicines)
                {
                    m.ProviderName = pDict.TryGetValue(m.ProviderId, out var name) ? name : "ID: " + m.ProviderId;
                }
            }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _medicineApi.DeleteAsync(id);
            return RedirectToPage();
        }
    }
}