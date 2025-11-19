using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Json;

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

                    // Guardamos token y datos básicos del usuario en cookies (HTTP only para token)
                    Response.Cookies.Append("AuthToken", auth.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddHours(8)
                    });

                    if (auth.User is not null)
                    {
                        Response.Cookies.Append("UserId", auth.User.id.ToString(), new CookieOptions { Expires = DateTimeOffset.UtcNow.AddHours(8) });
                        Response.Cookies.Append("Username", auth.User.username ?? string.Empty, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddHours(8) });
                        Response.Cookies.Append("UserRole", auth.User.role.ToString(), new CookieOptions { Expires = DateTimeOffset.UtcNow.AddHours(8) });
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
