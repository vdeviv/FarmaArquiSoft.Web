using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("usersApi", c =>
{
    c.BaseAddress = new Uri("https://localhost:7067");
});
builder.Services.AddHttpClient("clientsApi", c =>
{
    c.BaseAddress = new Uri("http://localhost:5142");
});
builder.Services.AddHttpClient("lotesApi", c =>
{
    c.BaseAddress = new Uri("http://localhost:5127");
});
builder.Services.AddHttpClient("providersApi", c =>
{
    c.BaseAddress = new Uri("http://localhost:5143");
});
builder.Services.AddHttpClient("medicinesApi", c =>
{
    c.BaseAddress = new Uri("http://localhost:5149"); 
});


builder.Services.AddScoped<MedicineApi>();
builder.Services.AddScoped<LotApi>();
builder.Services.AddScoped<UserApi>();
builder.Services.AddScoped<ClientApi>();
builder.Services.AddScoped<ProviderApi>();
builder.Services.AddScoped<AuthApi>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "FarmaAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        // En desarrollo dejamos SameAsRequest para evitar problemas con HTTP
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();

// Middleware que fuerza cambio de contraseña al primer login
app.Use(async (context, next) =>
{
    try
    {
        var user = context.User;
        var path = context.Request.Path;

        if (user?.Identity?.IsAuthenticated == true)
        {
            // 1) Intentar obtener token (claim o cookie)
            var token = user.FindFirst("access_token")?.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                context.Request.Cookies.TryGetValue("AuthToken", out token);
            }

            // 2) Si el JWT está expirado, invalidamos la cookie de autenticación y forzamos login
            if (!string.IsNullOrWhiteSpace(token) && IsJwtExpired(token))
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                // opcional: limpiar cookie de token
                context.Response.Cookies.Delete("AuthToken");
                context.Response.Redirect("/Auth/Login");
                return;
            }

            var hasChanged = user.FindFirst("HasChangedPassword")?.Value;
            var forceCookie = context.Request.Cookies["ForceChangePassword"];

            if (string.Equals(hasChanged, "false", StringComparison.OrdinalIgnoreCase) || forceCookie == "1")
            {
                var allowedPrefixes = new[]
                {
                    "/Auth/ChangePassword",
                    "/Auth/Logout",
                    "/Auth/Login",    // <-- permitir re-login
                    "/lib",
                    "/css",
                    "/js",
                    "/images",
                    "/favicon.ico"
                };

                var isAllowed = false;
                foreach (var p in allowedPrefixes)
                {
                    if (path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase))
                    {
                        isAllowed = true;
                        break;
                    }
                }

                if (!isAllowed)
                {
                    context.Response.Redirect("/Auth/ChangePassword");
                    return;
                }
            }
        }
    }
    catch
    {
        // En caso de error dejamos pasar la petición para evitar bloquear el sitio por un fallo del middleware.
    }

    await next();

    // Helper local para comprobar expiración simple del JWT (solo verifica "exp" del payload, no firma)
    static bool IsJwtExpired(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return false;
            var payload = parts[1];
            var mod = payload.Length % 4;
            if (mod != 0) payload += new string('=', 4 - mod);
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("exp", out var expEl) && expEl.ValueKind == JsonValueKind.Number)
            {
                var exp = expEl.GetInt64();
                var expDt = DateTimeOffset.FromUnixTimeSeconds(exp);
                return expDt <= DateTimeOffset.UtcNow;
            }
        }
        catch
        {
            // en caso de fallo no asumimos expirado
        }
        return false;
    }
});

app.UseAuthorization();
app.MapRazorPages();
app.Run();
