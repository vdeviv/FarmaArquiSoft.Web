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

        public Create(MedicineApi medicineApi, ProviderApi providerApi)
        {
            _medicineApi = medicineApi;
            _providerApi = providerApi;
        }

        [BindProperty]
        public MedicineDTO Medicine { get; set; } = new();

        public List<SelectListItem> ProviderOptions { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDependencies();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
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
                        },
                        mailPropertyName: "", idPropertyName: "",
                        mailKeywords: Array.Empty<string>(), idKeywords: Array.Empty<string>()
                    );

                    await LoadDependencies();
                    return Page();
                }

                ModelState.AddModelError(string.Empty, $"Error API: {(int)response.StatusCode} {response.ReasonPhrase}");
                await LoadDependencies();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                await LoadDependencies();
                return Page();
            }
        }

        private async Task LoadDependencies()
        {
            try
            {
                var providers = await _providerApi.GetAllAsync();
                ProviderOptions = providers
                    .Where(p => !p.is_deleted)
                    .Select(p => new SelectListItem
                    {
                        Value = p.id.ToString(),
                        Text = $"{p.first_name} {p.last_name}"
                    }).ToList();
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Error cargando proveedores.");
            }
        }
    }
}