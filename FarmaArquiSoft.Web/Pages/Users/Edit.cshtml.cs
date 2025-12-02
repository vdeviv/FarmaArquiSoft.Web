using System.Net;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FarmaArquiSoft.Web.Pages.Users
{
    public class Edit : PageModel
    {
        private readonly UserApi _userApi;

        public Edit(UserApi userApi)
        {
            _userApi = userApi;
        }

        [BindProperty]
        public UserDTO Usuario { get; set; } = new();

        public string DisplayUsername { get; set; } = string.Empty;

        public SelectList Roles { get; private set; } = default!;

        // Nuevos campos para el CI dividido
        [BindProperty]
        public string? CiPrefix { get; set; } = string.Empty;

        [BindProperty]
        public string? CiNumber { get; set; } = string.Empty;

        [BindProperty]
        public string? CiSuffix { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            LoadRoles();

            try
            {
                var dto = await _userApi.GetByIdAsync(id);

                if (dto == null)
                {
                    TempData["ErrorMessage"] = $"Usuario con ID {id} no encontrado.";
                    return RedirectToPage("Index");
                }

                Usuario = dto;
                DisplayUsername = dto.username ?? string.Empty;

                // Descomponer el CI existente en prefijo, número, sufijo
                SplitCi(dto.ci ?? string.Empty);

                return Page();
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión al cargar usuario: {ex.Message}";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocurrió un error inesperado: {ex.Message}";
                return RedirectToPage("Index");
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            LoadRoles();

            // Validar campos del CI (prefijo/sufijo solo letras, número solo dígitos)
            ValidateCiParts();

            if (!ModelState.IsValid)
                return Page();

            // Construir el CI completo a partir de los 3 campos
            Usuario.ci = $"{CiPrefix}{CiNumber}{CiSuffix}";

            try
            {
                var response = await _userApi.UpdateAsync(Usuario);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Usuario actualizado correctamente.";
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

                    // Volvemos a separar el CI en 3 partes por si vino modificado
                    SplitCi(Usuario.ci ?? string.Empty);

                    return Page();
                }

                ModelState.AddModelError(string.Empty,
                    $"Error al actualizar. Código: {(int)response.StatusCode}, Detalle: {response.ReasonPhrase}");
                return Page();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error de conexión con el API: {ex.Message}. Verifica que el servicio de Usuarios esté en ejecución.");
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

        /// <summary>
        /// Separa un CI tipo "AB13065677C" en prefijo, número, sufijo.
        /// </summary>
        private void SplitCi(string ci)
        {
            CiPrefix = string.Empty;
            CiNumber = string.Empty;
            CiSuffix = string.Empty;

            if (string.IsNullOrWhiteSpace(ci))
                return;

            int start = 0;
            int end = ci.Length - 1;

            // Prefijo: letras desde el inicio (máx 2)
            while (start <= end && char.IsLetter(ci[start]) && CiPrefix.Length < 2)
            {
                CiPrefix += ci[start];
                start++;
            }

            // Sufijo: letras desde el final (máx 2)
            var suffixChars = new List<char>();
            while (end >= start && char.IsLetter(ci[end]) && suffixChars.Count < 2)
            {
                suffixChars.Add(ci[end]);
                end--;
            }
            suffixChars.Reverse();
            CiSuffix = new string(suffixChars.ToArray());

            // Número central
            if (end >= start)
            {
                CiNumber = ci.Substring(start, end - start + 1);
            }
        }

        /// <summary>
        /// Valida que prefijo/sufijo sean solo letras y número solo dígitos.
        /// </summary>
        private void ValidateCiParts()
        {
            CiPrefix = CiPrefix?.Trim() ?? string.Empty;
            CiNumber = CiNumber?.Trim() ?? string.Empty;
            CiSuffix = CiSuffix?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(CiPrefix) && !CiPrefix.All(char.IsLetter))
            {
                ModelState.AddModelError(nameof(CiPrefix), "El prefijo solo puede contener letras.");
            }

            if (!string.IsNullOrEmpty(CiSuffix) && !CiSuffix.All(char.IsLetter))
            {
                ModelState.AddModelError(nameof(CiSuffix), "El sufijo solo puede contener letras.");
            }
        }
    }
}
