using GA_GymAssistant.Data;
using Microsoft.EntityFrameworkCore;
using GA_GymAssistant.Services;
using Microsoft.AspNetCore.Authentication.Cookies; // Necesario para AddCookie

var builder = WebApplication.CreateBuilder(args);

// 1. Base de Datos
var connectionString = builder.Configuration.GetConnectionString("GimpassDBConnection")
    ?? throw new InvalidOperationException("Connection string 'GimpassDBConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. REGISTRO DEL SERVICIO DE IA
builder.Services.AddHttpClient<IAGymService>();

// =======================================================
// 3. AGREGAR SERVICIOS DE AUTENTICACIÓN POR COOKIES
// =======================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Define la ruta a la que se redirigirá si un usuario no autorizado 
        // intenta acceder a una ruta con [Authorize]
        options.LoginPath = "/Auth/Login";
        // Opcional: Ruta para cuando el usuario sí está logueado pero no tiene permisos (Roles)
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Tiempo de vida de la cookie de sesión
    });

// 4. Controladores y Vistas
builder.Services.AddControllersWithViews();

// 5. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowFrontend",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// =======================================================
// 6. MIDDLEWARE: USAR AUTENTICACIÓN Y AUTORIZACIÓN
// Importante: app.UseAuthentication() DEBE ir antes de app.UseAuthorization()
// y después de app.UseRouting()
// =======================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowFrontend");

// HABILITADO
app.UseAuthentication();
// YA ESTABA PRESENTE
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();