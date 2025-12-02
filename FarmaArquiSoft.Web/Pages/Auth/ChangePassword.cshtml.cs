using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FarmaArquiSoft.Web.Pages.Auth
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserApi _userApi;

        // Ya no inyectamos IApiErrorAdapter, solo el UserApi
        public ChangePasswordModel(UserApi userApi)
        {
            _userApi = userApi;
        }

        [BindProperty]
        public ChangePasswordInputModel Input { get; set; } = new();

        public void OnGet()
        {
            var hasChanged = User.FindFirst("HasChangedPassword")?.Value;
            if (hasChanged == "false")
            {
                TempData["ForceMessage"] = "Por seguridad, debes cambiar tu contraseña temporal antes de continuar.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out int userId)) return Forbid();

            // Comprobaciones de Token
            var token = GetAccessTokenFromUserOrCookie();
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["ErrorMessage"] = "No se encontró token de autenticación. Vuelve a iniciar sesión.";
                return RedirectToPage("/Auth/Login");
            }

            if (IsJwtExpired(token))
            {
                TempData["ErrorMessage"] = "El token ha expirado. Vuelve a iniciar sesión.";
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                // 1. Llamada al API
                var response = await _userApi.ChangePasswordAsync(userId, Input.CurrentPassword, Input.NewPassword);

                if (response.IsSuccessStatusCode)
                {
                    await UpdateUserClaimsAsync();
                    TempData["SuccessMessage"] = "Contraseña actualizada correctamente.";
                    return RedirectToPage("/Index");
                }

                // 2. USO DE ApiValidationFacade (Estático)
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    // TRUCO: Reutilizamos los parámetros de 'mail' e 'id' del Facade 
                    // para mapear errores de 'CurrentPassword' y 'NewPassword' respectivamente.
                    ApiValidationFacade.MapValidationErrors(
                        modelState: ModelState,
                        jsonContent: jsonContent,
                        prefix: "Input",
                        fieldMap: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["CurrentPassword"] = "CurrentPassword",
                            ["NewPassword"] = "NewPassword",
                            ["ConfirmPassword"] = "ConfirmPassword"
                        },
                        // Usamos el espacio de "Mail" para mapear errores de la contraseña ACTUAL
                        mailPropertyName: "CurrentPassword",

                        // Usamos el espacio de "ID" (CI) para mapear errores de la NUEVA contraseña
                        idPropertyName: "NewPassword",

                        // Keywords para detectar errores de la contraseña actual
                        mailKeywords: new[] { "actual", "incorrecta", "vieja", "no coincide" },

                        // Keywords para detectar errores de complejidad en la nueva contraseña
                        idKeywords: new[] {
                            "contraseña", "password", "longitud", "caracteres",
                            "mayúscula", "minúscula", "número", "símbolo"
                        }
                    );

                    return Page();
                }

                // Manejo específico de 401
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"No autorizado: {(!string.IsNullOrWhiteSpace(errorBody) ? errorBody : "token inválido o contraseña actual incorrecta")}");
                    return Page();
                }

                // Otros errores del API
                var errorMsg = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Error del servidor: {errorMsg}");
                return Page();
            }
            catch (InvalidOperationException invEx)
            {
                ModelState.AddModelError(string.Empty, $"Error de autenticación interna: {invEx.Message}");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error de conexión: " + ex.Message);
                return Page();
            }
        }

        private async Task UpdateUserClaimsAsync()
        {
            var currentIdentity = User.Identity as ClaimsIdentity;
            if (currentIdentity == null) return;

            var existingClaim = currentIdentity.FindFirst("HasChangedPassword");
            if (existingClaim != null)
                currentIdentity.RemoveClaim(existingClaim);

            currentIdentity.AddClaim(new Claim("HasChangedPassword", "true"));

            var principal = new ClaimsPrincipal(currentIdentity);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        }

        private string? GetAccessTokenFromUserOrCookie()
        {
            var token = User.FindFirst("access_token")?.Value;
            if (!string.IsNullOrWhiteSpace(token)) return token;

            if (Request.Cookies.TryGetValue("AuthToken", out var cookieToken) && !string.IsNullOrWhiteSpace(cookieToken))
                return cookieToken;

            return null;
        }

        private bool IsJwtExpired(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2) return false;
                var payload = parts[1];
                var mod = payload.Length % 4;
                if (mod != 0) payload += new string('=', 4 - mod);
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("exp", out var expEl) && expEl.ValueKind == JsonValueKind.Number)
                {
                    var exp = expEl.GetInt64();
                    var expDt = DateTimeOffset.FromUnixTimeSeconds(exp);
                    return expDt <= DateTimeOffset.UtcNow;
                }
            }
            catch { }
            return false;
        }

        public class ChangePasswordInputModel
        {
            [Required(ErrorMessage = "La contraseña actual es requerida.")]
            public string CurrentPassword { get; set; } = "";

            [Required(ErrorMessage = "La nueva contraseña es requerida.")]
            public string NewPassword { get; set; } = "";

            [Required(ErrorMessage = "Debes confirmar la contraseña.")]
            [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; } = "";
        }
    }
}