using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FarmaArquiSoft.Web.DTOs; // Define aquí UserUpdateDTO y UserDTO (GET) si aún no existen
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace FarmaArquiSoft.Web.Pages.Users
{
    public class Edit : PageModel
    {
        private readonly HttpClient _http;

        public Edit(IHttpClientFactory factory)
        {
            // Usa el cliente que registraste (p.ej. "backendApi")
            _http = factory.CreateClient("backendApi");
        }

        [BindProperty]
        public UserDTO Usuario { get; set; } = new();

        public string DisplayUsername { get; set; } = string.Empty;

        public SelectList Roles { get; private set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            LoadRoles();

            try
            {
                var res = await _http.GetAsync($"/api/Users/{id}");
                if (res.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = $"Usuario con ID {id} no encontrado.";
                    return RedirectToPage("Index");
                }
                if (!res.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Error al cargar usuario. Código: {(int)res.StatusCode}, Detalle: {res.ReasonPhrase}";
                    return RedirectToPage("Index");
                }

                // Supón que el API devuelve un UserDTO con todas las propiedades para edición y lectura
                var dto = await res.Content.ReadFromJsonAsync<UserDTO>();
                if (dto == null)
                {
                    TempData["ErrorMessage"] = "La respuesta del API no contenía datos.";
                    return RedirectToPage("Index");
                }

                // Mapear DTO de lectura a DTO de edición
                Usuario = new UserDTO
                {
                    Id = dto.Id,
                    FirstName = dto.FirstName,
                    LastFirstName = dto.LastFirstName,
                    LastSecondName = dto.LastSecondName,
                    Mail = dto.Mail,
                    Phone = dto.Phone,
                    Ci = dto.Ci,
                    Role = dto.Role,
                    // Password queda vacío para no sobreescribir si no se cambia
                    IsActive = dto.IsActive
                };

                DisplayUsername = dto.Username ?? string.Empty;
                return Page();
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión al cargar usuario: {ex.Message}";
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
            LoadRoles();

            if (!ModelState.IsValid)
                return Page();

            try
            {
                var response = await _http.PutAsJsonAsync($"/api/Users/{Usuario.Id}", Usuario);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Usuario actualizado correctamente.";
                    return RedirectToPage("Index");
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    TryMapValidation(jsonContent, nameof(Usuario));
                    return Page();
                }

                ModelState.AddModelError(string.Empty, $"Error al actualizar. Código: {(int)response.StatusCode}, Detalle: {response.ReasonPhrase}");
                return Page();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error de conexión con el API: {ex.Message}. Verifica que el servicio de Usuarios esté en ejecución.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado: {ex.Message}");
                return Page();
            }
        }

        private void LoadRoles()
        {
            // Si prefieres, podrías consultarlos al API.
            Roles = new SelectList(Enum.GetValues(typeof(UserRole)));
        }

        private void TryMapValidation(string jsonContent, string prefix)
        {
            // Mapea { "error": "msg" } y { "errors": { "Field": ["msg1"] } }
            try
            {
                using var doc = JsonDocument.Parse(jsonContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("error", out var generalError) && generalError.ValueKind == JsonValueKind.String)
                    ModelState.AddModelError(string.Empty, generalError.GetString() ?? "Error de dominio no especificado.");

                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    foreach (var kv in errors.EnumerateObject())
                    {
                        var key = string.IsNullOrWhiteSpace(prefix) ? kv.Name : $"{prefix}.{kv.Name}";
                        if (kv.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var e in kv.Value.EnumerateArray())
                                ModelState.AddModelError(key, e.GetString() ?? e.ToString() ?? "Error de campo.");
                        }
                        else if (kv.Value.ValueKind == JsonValueKind.String)
                        {
                            ModelState.AddModelError(key, kv.Value.GetString() ?? "Error de campo.");
                        }
                    }
                }
            }
            catch { /* si no es JSON válido, ignoramos y queda el mensaje genérico */ }
        }
    }
}
