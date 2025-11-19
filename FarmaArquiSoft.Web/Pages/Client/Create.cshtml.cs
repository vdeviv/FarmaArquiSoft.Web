using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Json;
using System.Linq;

namespace FarmaArquiSoft.Web.Pages.Client
{
    public class Create : PageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty]
        public ClientDTO Cliente { get; set; } = new ClientDTO();

        public Create(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("clientsApi");
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Clients", Cliente);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cliente creado exitosamente.";
                    return RedirectToPage("./Index");
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    // Configuración específica para CLIENTES
                    var fieldMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["first_name"] = "first_name",
                        ["last_name"]  = "last_name",
                        ["nit"]        = "nit",
                        ["email"]      = "email"
                    };

                    ApiValidationFacade.MapValidationErrors(
                        modelState: ModelState,
                        jsonContent: jsonContent,
                        prefix: nameof(Cliente),
                        fieldMap: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["first_name"] = "first_name",
                            ["last_name"] = "last_name",
                            ["nit"] = "nit",
                            ["email"] = "email"
                        },
                        mailPropertyName: "email",
                        idPropertyName: "nit",
                        mailKeywords: new[] { "correo", "email", "mail" },
                        idKeywords: new[] { "nit", "c.i", "ci" }
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
                    $"Error de conexión con el API: {ex.Message}. Por favor, verifique que Clients.Api esté en ejecución.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Ocurrió un error inesperado al procesar la solicitud: {ex.Message}");
                return Page();
            }
        }
    }
}
