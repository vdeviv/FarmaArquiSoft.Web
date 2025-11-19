using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Json;
using System.Linq;


namespace FarmaArquiSoft.Web.Pages.Client
{
    public class Edit : PageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty]
        public ClientDTO Cliente { get; set; } = new ClientDTO();

        public Edit(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("clientsApi");
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<ClientDTO>($"/api/Clients/{id}");

                if (result == null)
                {
                    TempData["ErrorMessage"] = $"Cliente con ID {id} no encontrado.";
                    return RedirectToPage("./Index");
                }

                Cliente = result;
                return Page();
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] =
                    $"Error de conexión al cargar cliente: {ex.Message}";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    $"Ocurrió un error inesperado al cargar el cliente: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/Clients/{Cliente.id}", Cliente);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        $"Cliente '{Cliente.first_name} {Cliente.last_name}' actualizado correctamente.";
                    return RedirectToPage("./Index");
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    var fieldMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["first_name"] = "first_name",
                        ["last_name"] = "last_name",
                        ["nit"] = "nit",
                        ["email"] = "email"
                    };

                    ApiValidationFacade.MapValidationErrors(
                        modelState: ModelState,
                        jsonContent: jsonContent,
                        prefix: nameof(Cliente),
                        fieldMap: fieldMap,
                        mailPropertyName: "email",
                        idPropertyName: "nit",
                        mailKeywords: new[] { "correo", "email", "mail" },
                        idKeywords: new[] { "nit", "c.i", "ci" }
                    );

                    return Page();
                }

                TempData["ErrorMessage"] =
                    $"Error al actualizar el cliente. Código: {(int)response.StatusCode}, Detalle: {response.ReasonPhrase}";
                return RedirectToPage("./Index");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error de conexión con el API al intentar actualizar: {ex.Message}. Por favor, verifique que Clients.Api esté en ejecución.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Ocurrió un error inesperado: {ex.Message}");
                return Page();
            }
        }
    }
}
