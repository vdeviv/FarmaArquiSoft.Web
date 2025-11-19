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

// 👇 Registramos la fachada UserApi para DI
builder.Services.AddScoped<UserApi>();

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
app.UseAuthorization();

app.MapRazorPages();

app.Run();
