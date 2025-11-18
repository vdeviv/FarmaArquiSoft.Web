using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Text.Json;

namespace FarmaArquiSoft.Web.Pages.Client
{
    public class Index : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<ClientDTO> Clientes { get; set; } = new List<ClientDTO>();

        public Index(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("clientsApi");
        }

        public async Task OnGet()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<ClientDTO>>("/api/Clients");
                Clientes = result ?? new List<ClientDTO>();

                Clientes = Clientes.OrderBy(c => c.first_name).ToList();

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar clientes: {ex.Message}. Asegúrate que Clients.Api esté corriendo en el puerto 5142.");
                Clientes = new List<ClientDTO>();
            }
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/Clients/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Cliente con ID {id} eliminado (soft delete) correctamente.";
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = $"Cliente con ID {id} no encontrado.";
                }
                else
                {
                    string errorDetail = response.ReasonPhrase;
                    try
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                       
                        if (!string.IsNullOrEmpty(errorContent))
                        {
                            errorDetail = errorContent;
                        }
                    }
                    catch (Exception) { }

                    TempData["ErrorMessage"] = $"Error al eliminar el cliente. Código: {(int)response.StatusCode}. Detalle: {errorDetail}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión con el API al intentar eliminar: {ex.Message}";
            }

            return RedirectToPage();
        }

    }
}