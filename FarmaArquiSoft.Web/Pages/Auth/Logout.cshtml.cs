using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // Borramos cookies locales usadas para la sesión
            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("UserId");
            Response.Cookies.Delete("Username");
            Response.Cookies.Delete("UserRole");

            TempData["SuccessMessage"] = "Sesión cerrada correctamente.";
            return RedirectToPage("/Auth/Login");
        }
    }
}