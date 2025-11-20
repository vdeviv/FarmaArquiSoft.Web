using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddHttpClient("usersApi", c =>
{
    // Usa el puerto HTTPS del perfil "https" del User.Api
    c.BaseAddress = new Uri("https://localhost:7067");
});
builder.Services.AddScoped<ClientApi>();
// Microservicio de CLIENTES
builder.Services.AddHttpClient("clientsApi", c =>
{
    c.BaseAddress = new Uri("http://localhost:5142");
});

// Fachadas
builder.Services.AddScoped<UserApi>();
builder.Services.AddScoped<ClientApi>();

//Microservicio de LOTES
builder.Services.AddHttpClient("lotesApi", c =>
{
    c.BaseAddress = new Uri("http://localhost:5127"); 
});
builder.Services.AddScoped<LotApi>();

// -----------------------------
// Autenticación por cookies
// -----------------------------
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
// -----------------------------

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// Important: authentication middleware antes de authorization
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
            var hasChanged = user.FindFirst("HasChangedPassword")?.Value;
            var forceCookie = context.Request.Cookies["ForceChangePassword"];

            if (string.Equals(hasChanged, "false", StringComparison.OrdinalIgnoreCase) || forceCookie == "1")
            {
                var allowedPrefixes = new[]
                {
                    "/Auth/ChangePassword",
                    "/Auth/Logout",
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
});

app.UseAuthorization();

app.MapRazorPages();

app.Run();
