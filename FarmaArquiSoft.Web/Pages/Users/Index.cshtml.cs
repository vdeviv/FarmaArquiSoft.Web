using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Users
{
    [Authorize(Roles = "Administrador")]
    public class Index : PageModel
    {
        private readonly HttpClient _http;

        public Index(IHttpClientFactory factory)
        {
            // Usa el nombre del HttpClient que registraste (p.ej. "backendApi")
            _http = factory.CreateClient("backendApi");
        }

        public List<UserListItemDTO> Users { get; private set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var res = await _http.GetAsync("/api/Users");
                if (res.IsSuccessStatusCode)
                {
                    var list = await res.Content.ReadFromJsonAsync<List<UserListItemDTO>>();
                    Users = list ?? new();
                }
                else
                {
                    TempData["ErrorMessage"] = $"Error al cargar usuarios. Código: {(int)res.StatusCode}, Detalle: {res.ReasonPhrase}";
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
                var res = await _http.DeleteAsync($"/api/Users/{id}");
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
                    TempData["ErrorMessage"] = $"No se pudo eliminar. Código: {(int)res.StatusCode}, Detalle: {res.ReasonPhrase}";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión con el API al eliminar: {ex.Message}";
            }

            return RedirectToPage();
        }
    }

    // Ajusta las propiedades a lo que devuelva tu API en /api/Users
    public class UserListItemDTO
    {
        public int id { get; set; }
        public string username { get; set; } = "";
        public string first_name { get; set; } = "";
        public string last_first_name { get; set; } = "";
        public string last_second_name { get; set; } = "";
        public string? mail { get; set; }
        public string phone { get; set; } = "";
        public string ci { get; set; } = "";
        public string role { get; set; } = ""; // o enum si tu API devuelve enum
    }
}
