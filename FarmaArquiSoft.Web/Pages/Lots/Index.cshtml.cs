using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Lots
{
    public class IndexModel : PageModel
    {
        private readonly LotApi _lotApi;
        private readonly MedicineApi _medApi;

        public IndexModel(LotApi lotApi, MedicineApi medApi) { _lotApi = lotApi; _medApi = medApi; }

        public List<LotDTO> Lots { get; private set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var tLots = _lotApi.GetAllAsync();
                var tMeds = _medApi.GetAllAsync();
                await Task.WhenAll(tLots, tMeds);

                Lots = await tLots;
                var meds = await tMeds;
                var mDict = meds.ToDictionary(m => m.Id, m => $"{m.Name} ({m.Presentation})");

                foreach (var l in Lots)
                {
                    l.MedicineName = mDict.TryGetValue(l.medicine_id, out var name) ? name : "ID: " + l.medicine_id;
                }
            }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _lotApi.DeleteAsync(id);
            return RedirectToPage();
        }
    }
}