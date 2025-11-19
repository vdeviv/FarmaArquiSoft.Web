using System.Net;
using System.Net.Http.Json;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;


namespace FarmaArquiSoft.Web.Pages.Users
{
    public class Create : PageModel
    {
        private readonly UserApi _userApi;

        public Create(UserApi userApi)
        {
            _userApi = userApi;
        }

        [BindProperty]
        public UserDTO Usuario { get; set; } = new();

        public SelectList Roles { get; private set; } = default!;

        public void OnGet()
        {
            LoadRoles();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            LoadRoles();

            if (!ModelState.IsValid)
                return Page();

            try
            {
                var response = await _userApi.CreateAsync(Usuario);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Usuario creado correctamente. Se envió una contraseña temporal al correo.";
                    return RedirectToPage("Index");
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    ApiValidationFacade.MapValidationErrors(
                        modelState: ModelState,
                        jsonContent: jsonContent,
                        prefix: nameof(Usuario),
                        fieldMap: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["first_name"] = "first_name",
                            ["last_first_name"] = "last_first_name",
                            ["last_second_name"] = "last_second_name",
                            ["mail"] = "mail",
                            ["ci"] = "ci",
                            ["phone"] = "phone"
                        },
                        mailPropertyName: "mail",
                        idPropertyName: "ci",
                        mailKeywords: new[] { "correo", "mail" },
                        idKeywords: new[] { "ci", "carnet", "identidad" }
                    );

                    return Page();
                }

                ModelState.AddModelError(string.Empty,
                    $"Error inesperado del API. Código: {(int)response.StatusCode}, Detalle: {response.ReasonPhrase}");
                return Page();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error de conexión con el API: {ex.Message}. Verifica que el servicio de Usuarios esté en ejecución y la BaseAddress sea correcta.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado: {ex.Message}");
                return Page();
            }
        }

        private void LoadRoles()
        {
            Roles = new SelectList(Enum.GetValues(typeof(UserRole)));
        }
    }
}
