using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

// ESTA LÍNEA ES CRÍTICA: Debe coincidir con la ubicación de tu carpeta
namespace FarmaArquiSoft.Web.Pages.Sales
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SaleApi _saleApi;
        private readonly SaleDetailApi _saleDetailApi;
        private readonly MedicineApi _medicineApi;

        public List<SaleResponseDTO> Sales { get; set; } = new();

        public IndexModel(SaleApi saleApi, SaleDetailApi saleDetailApi, MedicineApi medicineApi)
        {
            _saleApi = saleApi;
            _saleDetailApi = saleDetailApi;
            _medicineApi = medicineApi;
        }

        public async Task OnGetAsync()
        {
            Sales = await _saleApi.GetAllAsync();
        }

        public async Task<JsonResult> OnGetSaleDetailsAsync(string id)
        {
            var tDetails = _saleDetailApi.GetBySaleIdAsync(id);
            var tMeds = _medicineApi.GetAllAsync();

            await Task.WhenAll(tDetails, tMeds);

            var details = await tDetails;
            var medicines = await tMeds;
            var medDict = medicines.ToDictionary(m => m.Id, m => m.Name);

            foreach (var d in details)
            {
                if (medDict.TryGetValue(d.MedicineId, out var medName))
                    d.MedicineName = medName;
                else
                    d.MedicineName = "ID: " + d.MedicineId;
            }

            return new JsonResult(details);
        }
    }
}