using System.Net;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FarmaArquiSoft.Web.Pages.Lots
{
    public class CreateModel : PageModel
    {
        private readonly LotApi _lotApi;
        private readonly MedicineApi _medicineApi; // Inyectamos MedicineApi

        [BindProperty]
        public LotDTO Input { get; set; } = new();

        // Lista para el ComboBox de Medicinas
        public List<SelectListItem> MedicineOptions { get; set; } = new();

        public CreateModel(LotApi lotApi, MedicineApi medicineApi)
        {
            _lotApi = lotApi;
            _medicineApi = medicineApi;
        }

        public async Task OnGetAsync()
        {
            // Fecha m?nima
            Input.expiration_date = DateTime.Today.AddMonths(6); // Sugerencia: 6 meses a futuro por defecto
            await LoadMedicines();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadMedicines();
                return Page();
            }

            try
            {
                var response = await _lotApi.CreateAsync(Input);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Lote creado exitosamente.";
                    return RedirectToPage("Index");
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    ApiValidationFacade.MapValidationErrors(
                        modelState: ModelState,
                        jsonContent: jsonContent,
                        prefix: nameof(Input),
                        fieldMap: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["batch_number"] = "batch_number",
                            ["medicine_id"] = "medicine_id", // Mapeamos el error de medicina
                            ["expiration_date"] = "expiration_date",
                            ["quantity"] = "quantity",
                            ["unit_cost"] = "unit_cost"
                        },
                        mailPropertyName: "", idPropertyName: "",
                        mailKeywords: Array.Empty<string>(), idKeywords: Array.Empty<string>()
                    );

                    await LoadMedicines();
                    return Page();
                }

                ModelState.AddModelError(string.Empty, $"Error API: {(int)response.StatusCode}");
                await LoadMedicines();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                await LoadMedicines();
                return Page();
            }
        }

        private async Task LoadMedicines()
        {
            try
            {
                var medicines = await _medicineApi.GetAllAsync();
                MedicineOptions = medicines
                    .Where(m => !m.IsDeleted)
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = $"{m.Name} ({m.Presentation})" // Mostramos Nombre + Presentaci?n
                    })
                    .OrderBy(m => m.Text)
                    .ToList();
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Error cargando lista de medicamentos.");
            }
        }
    }
}