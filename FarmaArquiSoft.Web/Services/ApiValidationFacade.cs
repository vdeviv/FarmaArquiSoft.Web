using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using System.Linq;


namespace FarmaArquiSoft.Web.Services
{
    public static class ApiValidationFacade
    {
        /// <summary>
        /// Mapea errores del API (DomainException y ValidationException)
        /// al ModelState del Razor Page.
        /// - Puede leer "message" o "error" como propiedad de dominio.
        /// - Permite parametrizar el nombre de la propiedad de correo y de documento (ci, nit, etc.)
        /// </summary>
        public static void MapValidationErrors(
            ModelStateDictionary modelState,
            string jsonContent,
            string prefix,
            Dictionary<string, string> fieldMap,
            string mailPropertyName,   // ej: "mail" o "email"
            string idPropertyName,     // ej: "ci" o "nit"
            string[] mailKeywords,
            string[] idKeywords
        )
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonContent);
                var root = doc.RootElement;

                // =========================================================
                // 1) DomainException => { "message": "..." } o { "error": "..." }
                // =========================================================
                if (TryGetDomainMessage(root, out var msg))
                {
                    // Ignoramos el mensaje genérico de ValidationException
                    if (!string.IsNullOrWhiteSpace(msg) &&
                        !msg.Contains("Validación de dominio falló", StringComparison.OrdinalIgnoreCase))
                    {
                        string modelStateKey = string.Empty;

                        // ¿Mensaje relacionado con correo?
                        if (mailKeywords.Any(k =>
                                msg.Contains(k, StringComparison.OrdinalIgnoreCase)))
                        {
                            modelStateKey = $"{prefix}.{mailPropertyName}";
                        }
                        // ¿Mensaje relacionado con CI/NIT/etc.?
                        else if (idKeywords.Any(k =>
                                     msg.Contains(k, StringComparison.OrdinalIgnoreCase)))
                        {
                            modelStateKey = $"{prefix}.{idPropertyName}";
                        }

                        // Si no matchea nada => error general (key vacío)
                        modelState.AddModelError(modelStateKey, msg);
                    }
                }

                // =========================================================
                // 2) ValidationException => { "errors": { field: "msg", ... } }
                // =========================================================
                if (root.TryGetProperty("errors", out var errorsElement) &&
                    errorsElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var kvp in errorsElement.EnumerateObject())
                    {
                        var apiFieldName = kvp.Name;   // ej: "first_name"
                        var value = kvp.Value;

                        // Mapeamos nombre devuelto por el API -> propiedad del DTO
                        string dtoPropName =
                            fieldMap.TryGetValue(apiFieldName, out var mapped)
                                ? mapped
                                : apiFieldName;

                        // Ej: "Cliente.first_name" o "Usuario.mail"
                        var modelStateKey = $"{prefix}.{dtoPropName}";

                        if (value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var err in value.EnumerateArray())
                            {
                                modelState.AddModelError(
                                    modelStateKey,
                                    err.GetString() ?? err.ToString() ?? "Error de campo.");
                            }
                        }
                        else if (value.ValueKind == JsonValueKind.String)
                        {
                            modelState.AddModelError(
                                modelStateKey,
                                value.GetString() ?? "Error de campo.");
                        }
                    }
                }
            }
            catch
            {
                // Si algo falla al parsear, no rompemos la página ni tiramos excepción.
            }
        }

        /// <summary>
        /// Intenta obtener el mensaje de dominio ya sea desde "message" o "error".
        /// </summary>
        private static bool TryGetDomainMessage(JsonElement root, out string message)
        {
            message = string.Empty;

            if (root.TryGetProperty("message", out var msgElement) &&
                msgElement.ValueKind == JsonValueKind.String)
            {
                message = msgElement.GetString() ?? string.Empty;
                return true;
            }

            if (root.TryGetProperty("error", out var errElement) &&
                errElement.ValueKind == JsonValueKind.String)
            {
                message = errElement.GetString() ?? string.Empty;
                return true;
            }

            return false;
        }
    }
}
