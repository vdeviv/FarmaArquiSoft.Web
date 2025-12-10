using System.Net;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FarmaArquiSoft.Web.Pages.Medicines
{
    public class Create : PageModel
    {
        private readonly MedicineApi _medicineApi;
        private readonly ProviderApi _providerApi;
        private readonly LotApi _lotApi;

        public Create(MedicineApi medicineApi, ProviderApi providerApi, LotApi lotApi)
        {
            _medicineApi = medicineApi;
            _providerApi = providerApi;
            _lotApi = lotApi;
        }

        [BindProperty]
        public MedicineDTO Medicine { get; set; } = new();

        // Listas para los ComboBox
        public List<SelectListItem> ProviderOptions { get; set; } = new();
        public List<LotDTO> AvailableLots { get; set; } = new();

        // Propiedad auxiliar para capturar los IDs de lotes seleccionados desde el Front
        [BindProperty]
        public List<int> SelectedLotIds { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDependencies();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            // Mapear los IDs de lotes seleccionados al objeto MedicineDTO
            if (SelectedLotIds != null && SelectedLotIds.Any())
            {
                Medicine.LinkedLots = SelectedLotIds
                    .Select(id => new MedicineLotLinkDTO { LotId = id })
                    .ToList();
            }

            if (!ModelState.IsValid)
            {
                await LoadDependencies();
                return Page();
            }

            try
            {
                var response = await _medicineApi.CreateAsync(Medicine);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Medicamento registrado correctamente.";
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
                            // Los errores de lotes suelen ser genéricos o en LinkedLots, 
                            // el facade los pondrá en el resumen o campo correspondiente.
                        },
                        mailPropertyName: "",
                        idPropertyName: "",
                        mailKeywords: Array.Empty<string>(),
                        idKeywords: Array.Empty<string>()
                    );

                    await LoadDependencies();
                    return Page();
                }

                ModelState.AddModelError(string.Empty,
                    $"Error inesperado del API. Código: {(int)response.StatusCode}, Detalle: {response.ReasonPhrase}");

                await LoadDependencies();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error de conexión o inesperado: {ex.Message}");
                await LoadDependencies();
                return Page();
            }
        }

        private async Task LoadDependencies()
        {
            try
            {
                // 1. Cargar Proveedores
                var providers = await _providerApi.GetAllAsync();
                ProviderOptions = providers
                    .Where(p => !p.is_deleted)
                    .Select(p => new SelectListItem
                    {
                        Value = p.id.ToString(),
                        Text = $"{p.first_name} {p.last_name}"
                    }).ToList();

                // 2. Cargar Lotes Disponibles
                var lots = await _lotApi.GetAllAsync();
                AvailableLots = lots.Where(l => !l.is_deleted && l.expiration_date > DateTime.Now).ToList();
            }
            catch
            {
                // Si falla la carga de dependencias, mostrar error pero no romper la página
                ModelState.AddModelError(string.Empty, "Error cargando listas de proveedores o lotes. Verifique la conexión.");
            }
        }
    }
}