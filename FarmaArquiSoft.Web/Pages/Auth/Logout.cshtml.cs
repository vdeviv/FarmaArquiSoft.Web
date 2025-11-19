using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FarmaArquiSoft.Web.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Sign out from cookie authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Borramos cookies con Path="/" para que coincida con las creadas en Login
            var options = new CookieOptions { Path = "/" };
            Response.Cookies.Delete("AuthToken", options);
            Response.Cookies.Delete("UserId", options);
            Response.Cookies.Delete("Username", options);
            Response.Cookies.Delete("UserRole", options);

            TempData["SuccessMessage"] = "Sesión cerrada correctamente.";
            return RedirectToPage("/Auth/Login");
        }
    }
}