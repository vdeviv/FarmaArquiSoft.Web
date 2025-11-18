using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Json;

namespace FarmaArquiSoft.Web.Pages.Users
{
    public class Index : PageModel
    {
        private readonly HttpClient _http;

        public Index(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("usersApi");
        }

        public List<UserDTO> Users { get; private set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var res = await _http.GetAsync("/api/user");
                if (res.IsSuccessStatusCode)
                {
                    var list = await res.Content.ReadFromJsonAsync<List<UserDTO>>();
                    Users = list ?? new();
                }
                else
                {
                    TempData["ErrorMessage"] =
                        $"Error al cargar usuarios. Código: {(int)res.StatusCode}, Detalle: {res.ReasonPhrase}";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión con el API: {ex.Message}";
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var res = await _http.DeleteAsync($"/api/user/{id}");

                if (res.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Usuario eliminado correctamente.";
                }
                else if (res.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = $"El usuario con ID {id} no existe.";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        $"No se pudo eliminar. Código: {(int)res.StatusCode}, Detalle: {res.ReasonPhrase}";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión con el API al eliminar: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
