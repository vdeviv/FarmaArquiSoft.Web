using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FarmaArquiSoft.Web.DTOs; // Define aquí tu UserCreateDTO si no existe aún
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;


    namespace FarmaArquiSoft.Web.Pages.Users
{
    public class Create : PageModel
    {
        private readonly HttpClient _httpClient;

        public Create(IHttpClientFactory factory)
        {
            // Usa el nombre del HttpClient que tengas registrado (p.ej. "backendApi")
            _httpClient = factory.CreateClient("backendApi");
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
                // Ajusta la ruta al endpoint real de tu API:
                var response = await _httpClient.PostAsJsonAsync("/api/Users", Usuario);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Usuario creado correctamente. Se envió una contraseña temporal al correo.";
                    return RedirectToPage("Index");
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    TryMapValidation(jsonContent, nameof(Usuario));
                    return Page();
                }

                ModelState.AddModelError(string.Empty, $"Error inesperado del API. Código: {(int)response.StatusCode}, Detalle: {response.ReasonPhrase}");
                return Page();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error de conexión con el API: {ex.Message}. Verifica que el servicio de Usuarios esté en ejecución y la BaseAddress sea correcta.");
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
            // Si prefieres cargarlos desde API, reemplaza por un GET al endpoint de roles.
            Roles = new SelectList(Enum.GetValues(typeof(UserRole)));
        }

        private void TryMapValidation(string jsonContent, string prefix)
        {
            // Mapea respuestas de validación con formato:
            // { "error": "mensaje" } o { "errors": { "Campo": ["msg1","msg2"] } }
            try
            {
                using var doc = JsonDocument.Parse(jsonContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("error", out var generalError) && generalError.ValueKind == JsonValueKind.String)
                {
                    ModelState.AddModelError(string.Empty, generalError.GetString() ?? "Error de dominio no especificado.");
                }

                if (root.TryGetProperty("errors", out var errorsElement) && errorsElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var kvp in errorsElement.EnumerateObject())
                    {
                        string apiFieldName = kvp.Name;
                        string modelStateKey = string.IsNullOrWhiteSpace(prefix) ? apiFieldName : $"{prefix}.{apiFieldName}";

                        if (kvp.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var err in kvp.Value.EnumerateArray())
                                ModelState.AddModelError(modelStateKey, err.GetString() ?? err.ToString() ?? "Error de campo.");
                        }
                        else if (kvp.Value.ValueKind == JsonValueKind.String)
                        {
                            ModelState.AddModelError(modelStateKey, kvp.Value.GetString() ?? "Error de campo.");
                        }
                    }
                }
            }
            catch
            {
                // Si la respuesta no es JSON válido, no interrumpe el flujo; quedará el mensaje genérico.
            }
        }
    }
}
