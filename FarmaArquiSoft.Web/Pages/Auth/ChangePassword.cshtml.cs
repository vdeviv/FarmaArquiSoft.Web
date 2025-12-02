using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FarmaArquiSoft.Web.Pages.Auth
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserApi _userApi;

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

            try
            {
                // 1. Llamada al API
                var response = await _userApi.ChangePasswordAsync(userId, Input.CurrentPassword, Input.NewPassword);

                if (response.IsSuccessStatusCode)
                {
                    // 2. IMPORTANTE: Actualizar la Cookie para que el Middleware sepa que ya cambió la contraseña
                    // Si no hacemos esto, el middleware lo seguirá redirigiendo aquí.
                    await UpdateUserClaimsAsync();

                    TempData["SuccessMessage"] = "Contraseña actualizada correctamente.";
                    return RedirectToPage("/Index");
                }

                // Manejo de errores del API
                var errorMsg = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Error: {errorMsg}"); // Simplificado, puedes parsear el JSON si quieres
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
            // Clonamos los claims actuales
            var currentIdentity = User.Identity as ClaimsIdentity;
            if (currentIdentity == null) return;

            // Removemos el claim viejo de HasChangedPassword
            var existingClaim = currentIdentity.FindFirst("HasChangedPassword");
            if (existingClaim != null)
                currentIdentity.RemoveClaim(existingClaim);

            // Agregamos el nuevo en 'true'
            currentIdentity.AddClaim(new Claim("HasChangedPassword", "true"));

            // Regeneramos la cookie
            var principal = new ClaimsPrincipal(currentIdentity);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        }

        public class ChangePasswordInputModel
        {
            [Required(ErrorMessage = "La contraseña actual es requerida.")]
            public string CurrentPassword { get; set; } = "";

            [Required(ErrorMessage = "La nueva contraseña es requerida.")]
            [MinLength(8, ErrorMessage = "Mínimo 8 caracteres.")]
            public string NewPassword { get; set; } = "";

            [Required(ErrorMessage = "Debes confirmar la contraseña.")]
            [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; } = "";
        }
    }
}