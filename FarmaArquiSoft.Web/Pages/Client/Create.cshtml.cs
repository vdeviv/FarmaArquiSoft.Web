using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace FarmaArquiSoft.Web.Pages.Client
{
    public class Create : PageModel
    {
        private readonly ClientApi _clientApi;

        [BindProperty]
        public ClientDTO Cliente { get; set; } = new();

        public Create(ClientApi clientApi)
        {
            _clientApi = clientApi;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var response = await _clientApi.CreateAsync(Cliente);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cliente creado exitosamente.";
                return RedirectToPage("./Index");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var json = await response.Content.ReadAsStringAsync();

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

            ModelState.AddModelError(string.Empty, $"Error inesperado {response.StatusCode}");
            return Page();
        }
    }
}
