using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FarmaArquiSoft.Web.DTOs;
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
            _http = factory.CreateClient("usersApi");
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
                var res = await _http.GetAsync($"/api/user/{id}");
                if (res.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = $"Usuario con ID {id} no encontrado.";
                    return RedirectToPage("Index");
                }
                if (!res.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        $"Error al cargar usuario. Código: {(int)res.StatusCode}, Detalle: {res.ReasonPhrase}";
                    return RedirectToPage("Index");
                }

                var dto = await res.Content.ReadFromJsonAsync<UserDTO>();
                if (dto == null)
                {
                    TempData["ErrorMessage"] = "La respuesta del API no contenía datos.";
                    return RedirectToPage("Index");
                }

                Usuario = dto;
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
                var response = await _http.PutAsJsonAsync($"/api/user/{Usuario.Id}", Usuario);

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

                ModelState.AddModelError(string.Empty,
                    $"Error al actualizar. Código: {(int)response.StatusCode}, Detalle: {response.ReasonPhrase}");
                return Page();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error de conexión con el API: {ex.Message}. Verifica que el servicio de Usuarios esté en ejecución.");
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
            Roles = new SelectList(Enum.GetValues(typeof(UserRole)));
        }

        private void TryMapValidation(string jsonContent, string prefix)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonContent);
                var root = doc.RootElement;

                // 1) DomainException -> { "message": "..." }
                if (root.TryGetProperty("message", out var msgElement) &&
                    msgElement.ValueKind == JsonValueKind.String)
                {
                    var msg = msgElement.GetString() ?? string.Empty;

                    // Ignoramos el mensaje genérico de ValidationException
                    if (!string.IsNullOrWhiteSpace(msg) &&
                        !msg.Contains("Validación de dominio falló", StringComparison.OrdinalIgnoreCase))
                    {
                        string modelStateKey;

                        if (msg.Contains("correo", StringComparison.OrdinalIgnoreCase) ||
                            msg.Contains("mail", StringComparison.OrdinalIgnoreCase))
                        {
                            // Mensajes de correo -> Mail
                            modelStateKey = string.IsNullOrWhiteSpace(prefix)
                                ? "Mail"
                                : $"{prefix}.Mail";
                        }
                        else if (msg.Contains("ci", StringComparison.OrdinalIgnoreCase) ||
                                 msg.Contains("carnet", StringComparison.OrdinalIgnoreCase) ||
                                 msg.Contains("identidad", StringComparison.OrdinalIgnoreCase))
                        {
                            // Mensajes de CI -> Ci
                            modelStateKey = string.IsNullOrWhiteSpace(prefix)
                                ? "Ci"
                                : $"{prefix}.Ci";
                        }
                        else
                        {
                            // Otros DomainException -> error general arriba
                            modelStateKey = string.Empty;
                        }

                        ModelState.AddModelError(modelStateKey, msg);
                    }
                }

                // 2) ValidationException -> { "errors": { "first_name": "...", ... } }
                if (root.TryGetProperty("errors", out var errorsElement) &&
                    errorsElement.ValueKind == JsonValueKind.Object)
                {
                    var fieldMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["first_name"] = "FirstName",
                        ["last_first_name"] = "LastFirstName",
                        ["last_second_name"] = "LastSecondName",
                        ["mail"] = "Mail",
                        ["ci"] = "Ci",
                        ["phone"] = "Phone"
                    };

                    foreach (var kvp in errorsElement.EnumerateObject())
                    {
                        var apiFieldName = kvp.Name;
                        var value = kvp.Value;

                        if (!fieldMap.TryGetValue(apiFieldName, out var dtoPropName))
                            dtoPropName = apiFieldName;

                        var modelStateKey = string.IsNullOrWhiteSpace(prefix)
                            ? dtoPropName
                            : $"{prefix}.{dtoPropName}";

                        if (value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var err in value.EnumerateArray())
                            {
                                ModelState.AddModelError(
                                    modelStateKey,
                                    err.GetString() ?? err.ToString() ?? "Error de campo.");
                            }
                        }
                        else if (value.ValueKind == JsonValueKind.String)
                        {
                            ModelState.AddModelError(
                                modelStateKey,
                                value.GetString() ?? "Error de campo.");
                        }
                    }
                }
            }
            catch
            {
                // Si algo falla al parsear, no rompemos la página
            }
        }
    }
}
