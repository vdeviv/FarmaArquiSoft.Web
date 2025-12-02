using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
                    var auth = await res.Content.ReadFromJsonAsync<AuthenticateResponseDTO>();
                    if (auth == null || string.IsNullOrWhiteSpace(auth.Token))
                    {
                        ModelState.AddModelError(string.Empty, "Respuesta inválida del servidor.");
                        return Page();
                    }

                    // Guardar JWT en cookie HttpOnly (solo servidor)
                    var tokenOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddHours(8),
                        Path = "/"
                    };
                    Response.Cookies.Append("AuthToken", auth.Token, tokenOptions);

                    // Crear ClaimsPrincipal y hacer SignIn
                    if (auth.User is not null)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, auth.User.id.ToString()),
                            new Claim(ClaimTypes.Name, auth.User.username ?? ""),
                            new Claim(ClaimTypes.Role, auth.User.role.ToString()),
                            new Claim("access_token", auth.Token),

                            // -----------------------------------------------------------
                            // 👇 NUEVO: Claim vital para el Middleware de seguridad
                            // -----------------------------------------------------------
                            new Claim("HasChangedPassword", auth.User.has_changed_password ? "true" : "false")
                        };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                        // Cookies legibles opcionales con datos públicos
                        var publicOptions = new CookieOptions
                        {
                            HttpOnly = false,
                            Secure = Request.IsHttps,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddHours(8),
                            Path = "/"
                        };

                        Response.Cookies.Append("UserId", auth.User.id.ToString(), publicOptions);
                        Response.Cookies.Append("Username", auth.User.username ?? string.Empty, publicOptions);
                        Response.Cookies.Append("UserRole", auth.User.role.ToString(), publicOptions);

                        // -----------------------------------------------------------
                        // 👇 NUEVO: Si debe cambiar contraseña, redirigir AHORA
                        // -----------------------------------------------------------
                        if (!auth.User.has_changed_password)
                        {
                            return RedirectToPage("/Auth/ChangePassword");
                        }
                    }

                    TempData["SuccessMessage"] = "Autenticación correcta.";
                    return LocalRedirect(ReturnUrl);
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