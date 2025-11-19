using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FarmaArquiSoft.Web.Pages.Auth
{
    public class ChangePasswordModel : PageModel
    {
        private readonly HttpClient _http;

        [BindProperty]
        public ChangePasswordRequestDTO Payload { get; set; } = new ChangePasswordRequestDTO();

        public ChangePasswordModel(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("usersApi");
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var token = Request.Cookies["AuthToken"];
            var userIdCookie = Request.Cookies["UserId"];

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userIdCookie) || !int.TryParse(userIdCookie, out var userId))
            {
                TempData["ErrorMessage"] = "Sesión inválida. Por favor, inicia sesión de nuevo.";
                return RedirectToPage("/Auth/Login");
            }

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var res = await _http.PostAsJsonAsync($"/api/user/{userId}/change-password", Payload);

                if (res.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Contraseña cambiada correctamente.";
                    return RedirectToPage("/Index");
                }

                if (res.StatusCode == HttpStatusCode.BadRequest)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    // Intentamos mapear errores simples
                    try
                    {
                        using var doc = JsonDocument.Parse(content);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                        {
                            ModelState.AddModelError(string.Empty, msg.GetString() ?? "Error en la operación.");
                        }
                        else if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var kvp in errors.EnumerateObject())
                            {
                                var key = kvp.Name;
                                var value = kvp.Value;

                                if (value.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var e in value.EnumerateArray())
                                        ModelState.AddModelError($"Payload.{key}", e.GetString() ?? e.ToString() ?? "Error de campo.");
                                }
                                else if (value.ValueKind == JsonValueKind.String)
                                {
                                    ModelState.AddModelError($"Payload.{key}", value.GetString() ?? "Error de campo.");
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Error de validación desconocido.");
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError(string.Empty, "Respuesta inválida del servidor.");
                    }

                    return Page();
                }

                var err = await res.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Error del servidor: {(int)res.StatusCode} {res.ReasonPhrase}. {err}");
                return Page();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error de conexión con el API: {ex.Message}");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error inesperado: {ex.Message}");
                return Page();
            }
        }
    }
}