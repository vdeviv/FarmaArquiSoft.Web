using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddHttpClient("usersApi", c =>
{
    // Usa el puerto HTTPS del perfil "https" del User.Api
    c.BaseAddress = new Uri("https://localhost:7067");
});

// Microservicio de CLIENTES
builder.Services.AddHttpClient("clientsApi", c =>
{
    c.BaseAddress = new Uri("http://localhost:5142");
});

// Fachadas
builder.Services.AddScoped<UserApi>();
builder.Services.AddScoped<ClientApi>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
