using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace FarmaArquiSoft.Web.Pages.Users
{
    public class Index : PageModel
    {
        private readonly UserApi _userApi;

        public Index(UserApi userApi)
        {
            _userApi = userApi;
        }

        public List<UserListItemDto> Users { get; private set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                Users = await _userApi.GetAllAsync();
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] =
                    $"Error de conexión con el API: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    $"Ocurrió un error inesperado al cargar usuarios: {ex.Message}";
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var res = await _userApi.DeleteAsync(id);

                if (res.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Usuario eliminado correctamente.";
                }
                else if (res.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = $"El usuario con ID {id} no existe.";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        $"No se pudo eliminar. Código: {(int)res.StatusCode}, Detalle: {res.ReasonPhrase}";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] =
                    $"Error de conexión con el API al eliminar: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    $"Error inesperado al eliminar: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
