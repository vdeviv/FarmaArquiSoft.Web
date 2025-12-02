using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FarmaArquiSoft.Web.Pages.Users
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserApi _userApi;

        public ProfileModel(UserApi userApi)
        {
            _userApi = userApi;
        }

        public UserDTO CurrentUser { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out int id)) return RedirectToPage("/Auth/Login");

            try
            {
                var user = await _userApi.GetByIdAsync(id);
                if (user == null) return NotFound();
                CurrentUser = user;
                return Page();
            }
            catch
            {
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out int id)) return BadRequest();

            try
            {
                var response = await _userApi.DeleteAsync(id);

                if (response.IsSuccessStatusCode)
                {
                    // Cerrar sesión localmente tras borrar en el API
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    TempData["SuccessMessage"] = "Tu cuenta ha sido desactivada correctamente.";
                    return RedirectToPage("/Auth/Login");
                }

                TempData["ErrorMessage"] = "No se pudo desactivar la cuenta.";
                return RedirectToPage();
            }
            catch
            {
                TempData["ErrorMessage"] = "Error de conexión al intentar borrar la cuenta.";
                return RedirectToPage();
            }
        }
    }
}