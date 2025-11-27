using System.Net;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Providers
{
    public class Edit : PageModel
    {
        private readonly ProviderApi _providerApi;

        public Edit(ProviderApi providerApi)
        {
            _providerApi = providerApi;
        }

        [BindProperty]
        public ProviderDTO Proveedor { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var dto = await _providerApi.GetByIdAsync(id);

                if (dto == null)
                {
                    TempData["ErrorMessage"] = $"Proveedor con ID {id} no encontrado.";
                    return RedirectToPage("Index");
                }

                Proveedor = dto;
                return Page();
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión al cargar proveedor: {ex.Message}";
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
                var response = await _providerApi.UpdateAsync(Proveedor);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Proveedor actualizado correctamente.";
                    return RedirectToPage("Index");
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    ApiValidationFacade.MapValidationErrors(
                        modelState: ModelState,
                        jsonContent: jsonContent,
                        prefix: nameof(Proveedor),
                        fieldMap: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["first_name"] = "first_name",
                            ["last_name"] = "last_name",
                            ["email"] = "email",
                            ["phone"] = "phone"
                        },
                        mailPropertyName: "email",
                        idPropertyName: "phone",
                        mailKeywords: new[] { "correo", "mail", "email" },
                        idKeywords: new[] { "telefono", "teléfono", "phone", "celular" }
                    );

                    return Page();
                }

                ModelState.AddModelError(string.Empty,
                    $"Error al actualizar proveedor. Código: {(int)response.StatusCode}, Detalle: {response.ReasonPhrase}");
                return Page();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error de conexión con el API de Proveedores: {ex.Message}. Verifica que el microservicio esté en ejecución.");
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
