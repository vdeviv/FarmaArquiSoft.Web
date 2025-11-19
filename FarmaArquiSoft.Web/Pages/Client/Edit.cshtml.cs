using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace FarmaArquiSoft.Web.Pages.Client
{
    public class Edit : PageModel
    {
        private readonly ClientApi _clientApi;

        [BindProperty]
        public ClientDTO Cliente { get; set; } = new();

        public Edit(ClientApi clientApi)
        {
            _clientApi = clientApi;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var dto = await _clientApi.GetByIdAsync(id);

            if (dto == null)
            {
                TempData["ErrorMessage"] = $"Cliente con ID {id} no encontrado.";
                return RedirectToPage("./Index");
            }

            Cliente = dto;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var res = await _clientApi.UpdateAsync(Cliente);

            if (res.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cliente actualizado correctamente.";
                return RedirectToPage("./Index");
            }

            if (res.StatusCode == HttpStatusCode.BadRequest)
            {
                var json = await res.Content.ReadAsStringAsync();

                ApiValidationFacade.MapValidationErrors(
                    ModelState,
                    json,
                    nameof(Cliente),
                    new Dictionary<string, string>
                    {
                        ["first_name"] = "first_name",
                        ["last_name"] = "last_name",
                        ["nit"] = "nit",
                        ["email"] = "email"
                    },
                    mailPropertyName: "email",
                    idPropertyName: "nit",
                    mailKeywords: new[] { "correo", "email", "mail" },
                    idKeywords: new[] { "nit", "ci" }
                );

                return Page();
            }

            ModelState.AddModelError(string.Empty, $"Error inesperado {res.StatusCode}");
            return Page();
        }
    }
}
