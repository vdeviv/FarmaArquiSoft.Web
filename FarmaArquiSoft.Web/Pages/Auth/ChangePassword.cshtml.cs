using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
            if (User?.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Auth/Login");

            var force = (Request.Cookies["ForceChangePassword"] == "1")
                        || (User?.FindFirst("HasChangedPassword")?.Value == "false");

            if (!force)
                return RedirectToPage("/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var token = Request.Cookies["AuthToken"];
            var userIdCookie = Request.Cookies["UserId"];

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userIdCookie) || !int.TryParse(userIdCookie, out var userId))
            {
                TempData["ErrorMessage"] = "Sesi�ón inválida. Por favor, inicia sesión de nuevo.";
                return RedirectToPage("/Auth/Login");
            }

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var res = await _http.PostAsJsonAsync($"/api/user/{userId}/change-password", Payload);

                if (res.IsSuccessStatusCode)
                {
                     try
                    {
                        var userRes = await _http.GetAsync($"/api/user/{userId}");
                        string? content = null;
                        bool hasChangedPassword = true;
                        string pwdVer = "0";
                        string username = "";
                        string roleStr = "";

                        if (userRes.IsSuccessStatusCode)
                        {
                            content = await userRes.Content.ReadAsStringAsync();
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                using var doc = JsonDocument.Parse(content);
                                var root = doc.RootElement;

                                if (root.ValueKind == JsonValueKind.Object)
                                {
                                    if (root.TryGetProperty("has_changed_password", out var hcp) && (hcp.ValueKind == JsonValueKind.True || hcp.ValueKind == JsonValueKind.False))
                                        hasChangedPassword = hcp.GetBoolean();

                                    if (root.TryGetProperty("password_version", out var pv) && pv.ValueKind == JsonValueKind.Number)
                                        pwdVer = pv.GetInt32().ToString();

                                    if (root.TryGetProperty("username", out var un) && un.ValueKind == JsonValueKind.String)
                                        username = un.GetString() ?? "";

                                    if (root.TryGetProperty("role", out var rl) && rl.ValueKind == JsonValueKind.String)
                                        roleStr = rl.GetString() ?? "";
                                }
                            }
                        }

                       var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                            new Claim(ClaimTypes.Name, username),
                            new Claim(ClaimTypes.Role, roleStr)
                        };

                        claims.Add(new Claim("HasChangedPassword", hasChangedPassword ? "true" : "false"));
                        claims.Add(new Claim("PwdVer", pwdVer));

                        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));

                        Response.Cookies.Delete("ForceChangePassword", new CookieOptions { Path = "/" });

                        TempData["SuccessMessage"] = "Contraseña cambiada correctamente.";
                        return RedirectToPage("/Index");
                    }
                    catch
                    {
                        Response.Cookies.Delete("ForceChangePassword", new CookieOptions { Path = "/" });
                        TempData["SuccessMessage"] = "Contraseña cambiada correctamente.";
                        return RedirectToPage("/Index");
                    }
                }

                if (res.StatusCode == HttpStatusCode.BadRequest)
                {
                    var content = await res.Content.ReadAsStringAsync();

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
                            ModelState.AddModelError(string.Empty, "Error de validaci�n desconocido.");
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError(string.Empty, "Respuesta inv�lida del servidor.");
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