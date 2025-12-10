using System.Net;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FarmaArquiSoft.Web.Pages.Medicines
{
    public class Edit : PageModel
    {
        private readonly MedicineApi _medicineApi;
        private readonly ProviderApi _providerApi;
        private readonly LotApi _lotApi;

        public Edit(MedicineApi medicineApi, ProviderApi providerApi, LotApi lotApi)
        {
            _medicineApi = medicineApi;
            _providerApi = providerApi;
            _lotApi = lotApi;
        }

        [BindProperty]
        public MedicineDTO Medicine { get; set; } = new();

        public List<SelectListItem> ProviderOptions { get; set; } = new();
        public List<LotDTO> AvailableLots { get; set; } = new();

        [BindProperty]
        public List<int> SelectedLotIds { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var dto = await _medicineApi.GetByIdAsync(id);
                if (dto == null)
                {
                    TempData["ErrorMessage"] = "Medicamento no encontrado.";
                    return RedirectToPage("Index");
                }
                Medicine = dto;

                // Pre-cargar los lotes que ya tiene asignados para que el JS los pinte
                if (Medicine.LinkedLots != null)
                {
                    SelectedLotIds = Medicine.LinkedLots.Select(l => l.LotId).ToList();
                }

                await LoadDependencies();
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar: {ex.Message}";
                return RedirectToPage("Index");
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            // Mapeo manual de la lista de IDs al objeto DTO
            if (SelectedLotIds != null)
            {
                Medicine.LinkedLots = SelectedLotIds
                    .Select(id => new MedicineLotLinkDTO { LotId = id, MedicineId = Medicine.Id })
                    .ToList();
            }

            if (!ModelState.IsValid)
            {
                await LoadDependencies();
                return Page();
            }

            try
            {
                var response = await _medicineApi.UpdateAsync(Medicine);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Medicamento actualizado correctamente.";
                    return RedirectToPage("Index");
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    ApiValidationFacade.MapValidationErrors(
                        modelState: ModelState,
                        jsonContent: jsonContent,
                        prefix: nameof(Medicine),
                        fieldMap: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["name"] = "Name",
                            ["presentation"] = "Presentation",
                            ["provider_id"] = "ProviderId"
                        },
                        mailPropertyName: "", idPropertyName: "",
                        mailKeywords: Array.Empty<string>(), idKeywords: Array.Empty<string>()
                    );

                    await LoadDependencies();
                    return Page();
                }

                ModelState.AddModelError(string.Empty, $"Error API: {response.StatusCode}");
                await LoadDependencies();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error inesperado: {ex.Message}");
                await LoadDependencies();
                return Page();
            }
        }

        private async Task LoadDependencies()
        {
            var providers = await _providerApi.GetAllAsync();
            ProviderOptions = providers.Where(p => !p.is_deleted).Select(p => new SelectListItem
            {
                Value = p.id.ToString(),
                Text = $"{p.first_name} {p.last_name}"
            }).ToList();

            var lots = await _lotApi.GetAllAsync();
            AvailableLots = lots.Where(l => !l.is_deleted).ToList();
        }
    }
}