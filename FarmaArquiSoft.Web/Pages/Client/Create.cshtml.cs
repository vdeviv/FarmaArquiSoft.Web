using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json; 
using System;

namespace FarmaArquiSoft.Web.Pages.Client
{
    public class Create : PageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty]
        public ClientDTO Cliente { get; set; } = new ClientDTO();

        public Create(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("clienteApi");
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Clients", Cliente);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cliente creado exitosamente.";
                    return RedirectToPage("./Index");
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"[API DEBUG] Raw Error JSON from API (HTTP 400): {jsonContent}");

                    try
                    {
                        using (var doc = JsonDocument.Parse(jsonContent))
                        {
                            JsonElement rootElement = doc.RootElement;
                            JsonElement errorsToProcess = rootElement; 

                            if (rootElement.TryGetProperty("error", out JsonElement generalError) && generalError.ValueKind == JsonValueKind.String)
                            {
                                ModelState.AddModelError(string.Empty, generalError.GetString() ?? "Error de dominio no especificado.");
                            }
                            else if (rootElement.TryGetProperty("errors", out JsonElement errorsElement) && errorsElement.ValueKind == JsonValueKind.Object)
                            {
                                errorsToProcess = errorsElement;
                            }

                            if (errorsToProcess.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var kvp in errorsToProcess.EnumerateObject())
                                {
                                    string apiFieldName = kvp.Name;

                                    string modelStateKey = $"{nameof(Cliente)}.{apiFieldName}";

                                    if (kvp.Value.ValueKind == JsonValueKind.Array)
                                    {
                                        foreach (var errorArrayElement in kvp.Value.EnumerateArray())
                                        {
                                            ModelState.AddModelError(modelStateKey, errorArrayElement.GetString() ?? errorArrayElement.ToString() ?? "Error de campo.");
                                        }
                                    }
                                    else if (kvp.Value.ValueKind == JsonValueKind.String)
                                    {
                                        ModelState.AddModelError(modelStateKey, kvp.Value.GetString() ?? "Error de campo.");
                                    }
                                }
                            }
                            else if (!rootElement.TryGetProperty("error", out _))
                            {
                                ModelState.AddModelError(string.Empty, $"Error de validación del API con formato inesperado. Contenido: {jsonContent}");
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        ModelState.AddModelError(string.Empty, $"El API devolvió un HTTP 400, pero la respuesta no es JSON válido: {jsonContent}. Detalle: {ex.Message}");
                    }

                    return Page();
                }

                ModelState.AddModelError(string.Empty, $"Error inesperado del API. Código: {(int)response.StatusCode}, Detalle: {response.ReasonPhrase}");
                return Page();

            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error de conexión con el API: {ex.Message}. Por favor, verifique que Clients.Api esté en ejecución.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al procesar la solicitud: {ex.Message}");
                return Page();
            }
        }
    }
}