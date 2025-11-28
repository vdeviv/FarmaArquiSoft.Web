using System.Net.Http.Json;
using System.Text.Json;

namespace FarmaArquiSoft.Web.Services
{
    public class AuthApi
    {
        private readonly HttpClient _http;

        public AuthApi(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("usersApi");
        }

        public record AuthResponse(string token, AuthUser user);
        public record AuthUser(int id, string username, string lastFirstName, string? lastSecondName, string? mail, string phone, string ci, string role);

        public async Task<AuthResponse?> AuthenticateAsync(string username, string password)
        {
            var body = new { Username = username, Password = password };

            var res = await _http.PostAsJsonAsync("/api/user/authenticate", body);

            if (!res.IsSuccessStatusCode)
                return null;

            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("token", out var tokenProp) || tokenProp.ValueKind != JsonValueKind.String)
                return null;

            var token = tokenProp.GetString() ?? string.Empty;

            var userElement = root.GetProperty("user");
            var authUser = new AuthUser(
                id: userElement.GetProperty("id").GetInt32(),
                username: userElement.GetProperty("username").GetString() ?? "",
                lastFirstName: userElement.GetProperty("lastFirstName").GetString() ?? "",
                lastSecondName: userElement.TryGetProperty("lastSecondName", out var lf) ? lf.GetString() : null,
                mail: userElement.TryGetProperty("mail", out var m) ? m.GetString() : null,
                phone: userElement.GetProperty("phone").GetString() ?? "",
                ci: userElement.GetProperty("ci").GetString() ?? "",
                role: userElement.GetProperty("role").GetString() ?? ""
            );

            return new AuthResponse(token, authUser);
        }
    }
}
