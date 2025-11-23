using System.Net;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Lots
{
    public class CreateModel : PageModel
    {
        private readonly LotApi _api;

        [BindProperty]
        public LotDTO Input { get; set; } = new();

        public CreateModel(LotApi api)
        {
            _api = api;
        }

        public void OnGet()
        {
            // Fecha mínima para evitar fechas "0001-01-01"
            Input.expiration_date = DateTime.Today;
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var response = await _api.CreateAsync(Input);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Lote creado exitosamente.";
                    return RedirectToPage("Index");
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    // Reutilizamos ApiValidationFacade igual que en Users
                    ApiValidationFacade.MapValidationErrors(
                        modelState: ModelState,
                        jsonContent: jsonContent,
                        prefix: nameof(Input),
                        fieldMap: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["batch_number"] = "batch_number",
                            ["expiration_date"] = "expiration_date",
                            ["quantity"] = "quantity",
                            ["unit_cost"] = "unit_cost"
                        },
                        mailPropertyName: "batch_number",   // no usamos mail, solo para cumplir firma
                        idPropertyName: "batch_number",
                        mailKeywords: Array.Empty<string>(),
                        idKeywords: Array.Empty<string>()
                    );

                    return Page();
                }

                ModelState.AddModelError(string.Empty,
                    $"Error inesperado del API. Código: {(int)response.StatusCode}, Detalle: {response.ReasonPhrase}");
                return Page();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error de conexión con el API de lotes: {ex.Message}");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado: {ex.Message}");
                return Page();
            }
        }
    }
}
