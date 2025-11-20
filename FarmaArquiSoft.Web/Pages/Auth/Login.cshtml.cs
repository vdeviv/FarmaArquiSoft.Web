using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace FarmaArquiSoft.Web.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly HttpClient _http;

        [BindProperty]
        public AuthenticateRequestDTO Credentials { get; set; } = new AuthenticateRequestDTO();

        public string ReturnUrl { get; set; } = "/";

        public LoginModel(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("usersApi");
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl!;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl!;

            if (!ModelState.IsValid) return Page();

            try
            {
                var res = await _http.PostAsJsonAsync("/api/user/authenticate", Credentials);

                if (res.IsSuccessStatusCode)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        ModelState.AddModelError(string.Empty, "Respuesta inválida del servidor.");
                        return Page();
                    }

                    try
                    {
                        using var doc = JsonDocument.Parse(content);
                        var root = doc.RootElement;

                        var token = root.TryGetProperty("token", out var t) && t.ValueKind == JsonValueKind.String
                            ? t.GetString()
                            : null;

                        if (string.IsNullOrWhiteSpace(token))
                        {
                            ModelState.AddModelError(string.Empty, "Token no presente en la respuesta del servidor.");
                            return Page();
                        }

                        // Extraer info del usuario (si viene)
                        bool hasChangedPassword = true;
                        string userIdStr = string.Empty;
                        string username = string.Empty;
                        string roleStr = string.Empty;

                        JsonElement? userElemNullable = null;
                        if (root.TryGetProperty("user", out var userElem) && userElem.ValueKind == JsonValueKind.Object)
                            userElemNullable = userElem;

                        // Si no hay user, quizá la API devuelve usuario en otra propiedad; se queda null y seguiremos buscando en root.
                        if (userElemNullable.HasValue)
                        {
                            var ue = userElemNullable.Value;
                            if (ue.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number && idProp.TryGetInt32(out var idv))
                                userIdStr = idv.ToString();

                            if (ue.TryGetProperty("username", out var un) && un.ValueKind == JsonValueKind.String)
                                username = un.GetString() ?? string.Empty;

                            if (ue.TryGetProperty("role", out var rl) && rl.ValueKind == JsonValueKind.String)
                                roleStr = rl.GetString() ?? string.Empty;

                            if (ue.TryGetProperty("has_changed_password", out var hcp))
                            {
                                switch (hcp.ValueKind)
                                {
                                    case JsonValueKind.True:
                                    case JsonValueKind.False:
                                        hasChangedPassword = hcp.GetBoolean();
                                        break;
                                    case JsonValueKind.Number:
                                        if (hcp.TryGetInt32(out var iv)) hasChangedPassword = iv != 0;
                                        break;
                                    case JsonValueKind.String:
                                        var s = hcp.GetString();
                                        if (bool.TryParse(s, out var bv)) hasChangedPassword = bv;
                                        else if (int.TryParse(s, out var iv2)) hasChangedPassword = iv2 != 0;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            // Intentar leer has_changed_password en la raíz si la API lo devolviera ahí
                            if (root.TryGetProperty("has_changed_password", out var hcpRoot))
                            {
                                switch (hcpRoot.ValueKind)
                                {
                                    case JsonValueKind.True:
                                    case JsonValueKind.False:
                                        hasChangedPassword = hcpRoot.GetBoolean();
                                        break;
                                    case JsonValueKind.Number:
                                        if (hcpRoot.TryGetInt32(out var iv)) hasChangedPassword = iv != 0;
                                        break;
                                    case JsonValueKind.String:
                                        var s = hcpRoot.GetString();
                                        if (bool.TryParse(s, out var bv)) hasChangedPassword = bv;
                                        else if (int.TryParse(s, out var iv2)) hasChangedPassword = iv2 != 0;
                                        break;
                                }
                            }
                        }

                        // Guardar JWT (opcional) en cookie HttpOnly
                        var tokenOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = Request.IsHttps,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddHours(8),
                            Path = "/"
                        };
                        Response.Cookies.Append("AuthToken", token, tokenOptions);

                        // Crear ClaimsPrincipal y hacer SignIn (cookie auth) para poblar User.Identity
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, string.IsNullOrWhiteSpace(userIdStr) ? "0" : userIdStr),
                            new Claim(ClaimTypes.Name, username ?? ""),
                            new Claim(ClaimTypes.Role, roleStr ?? "")
                        };

                        // Claim adicional para controlar cambio de password
                        claims.Add(new Claim("HasChangedPassword", hasChangedPassword ? "true" : "false"));

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                        // Opcional: cookies legibles con datos públicas
                        var publicOptions = new CookieOptions
                        {
                            HttpOnly = false,
                            Secure = Request.IsHttps,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddHours(8),
                            Path = "/"
                        };

                        if (!string.IsNullOrWhiteSpace(userIdStr))
                            Response.Cookies.Append("UserId", userIdStr, publicOptions);
                        Response.Cookies.Append("Username", username ?? string.Empty, publicOptions);
                        Response.Cookies.Append("UserRole", roleStr ?? string.Empty, publicOptions);

                        // Si es primera vez (no ha cambiado), obligar a cambiar contraseña
                        if (!hasChangedPassword)
                        {
                            var forceOptions = new CookieOptions
                            {
                                HttpOnly = false,
                                Secure = Request.IsHttps,
                                SameSite = SameSiteMode.Lax,
                                Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                                Path = "/"
                            };
                            Response.Cookies.Append("ForceChangePassword", "1", forceOptions);
                            TempData["InfoMessage"] = "Debes cambiar tu contraseña antes de continuar.";
                            return RedirectToPage("/Auth/ChangePassword");
                        }

                        TempData["SuccessMessage"] = "Autenticación correcta.";
                        return LocalRedirect(ReturnUrl);
                    }
                    catch (JsonException)
                    {
                        ModelState.AddModelError(string.Empty, "Respuesta JSON inválida del servidor.");
                        return Page();
                    }
                }

                if (res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, !string.IsNullOrWhiteSpace(content) ? content : "Usuario o contraseña incorrectos.");
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