using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Microservicio de USUARIOS 
builder.Services.AddHttpClient("usersApi", c =>
{
    c.BaseAddress = new Uri("http://localhost:5031"); 
});

// ✅ Microservicio de CLIENTES
builder.Services.AddHttpClient("clientsApi", c =>
{
    c.BaseAddress = new Uri("http://localhost:5142"); 
});

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