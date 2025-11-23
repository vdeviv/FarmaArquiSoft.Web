using System.Net;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Lots
{
    public class EditModel : PageModel
    {
        private readonly LotApi _api;

        [BindProperty]
        public LotDTO Input { get; set; } = new();

        public EditModel(LotApi api)
        {
            _api = api;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "ID inválido.";
                return RedirectToPage("Index");
            }

            try
            {
                var lot = await _api.GetByIdAsync(id);
                if (lot == null)
                {
                    TempData["ErrorMessage"] = "Lote no encontrado.";
                    return RedirectToPage("Index");
                }

                Input = lot;
                if (Input.expiration_date < DateTime.Today)
                    Input.expiration_date = DateTime.Today;

                return Page();
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión al cargar lote: {ex.Message}";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocurrió un error inesperado: {ex.Message}";
                return RedirectToPage("Index");
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var response = await _api.UpdateAsync(Input);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Lote actualizado correctamente.";
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
                            ["expiration_date"] = "expiration_date",
                            ["quantity"] = "quantity",
                            ["unit_cost"] = "unit_cost"
                        },
                        mailPropertyName: "batch_number",
                        idPropertyName: "batch_number",
                        mailKeywords: Array.Empty<string>(),
                        idKeywords: Array.Empty<string>()
                    );

                    return Page();
                }

                ModelState.AddModelError(string.Empty,
                    $"Error al actualizar lote. Código: {(int)response.StatusCode}, Detalle: {response.ReasonPhrase}");
                return Page();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error de conexión con el API: {ex.Message}");
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
