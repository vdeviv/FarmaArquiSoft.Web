using System.Net;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Providers
{
    public class Create : PageModel
    {
        private readonly ProviderApi _providerApi;

        public Create(ProviderApi providerApi)
        {
            _providerApi = providerApi;
        }

        [BindProperty]
        public ProviderDTO Proveedor { get; set; } = new();

        public void OnGet()
        {
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var response = await _providerApi.CreateAsync(Proveedor);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Proveedor creado correctamente.";
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
                    $"Error inesperado del API. Código: {(int)response.StatusCode}, Detalle: {response.ReasonPhrase}");
                return Page();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error de conexión con el API de Proveedores: {ex.Message}. Verifica que el microservicio de Proveedores esté en ejecución y la BaseAddress sea correcta.");
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
