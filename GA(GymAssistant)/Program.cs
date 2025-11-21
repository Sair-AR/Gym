using GA_GymAssistant.Data; // Importa tu contexto de datos
using GA_GymAssistant.Services; // Importa tu servicio de IA
using Microsoft.EntityFrameworkCore; // Necesario para AddDbContext y UseSqlServer

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. CONFIGURACIÓN DE CONEXIÓN A LA BASE DE DATOS (DBContext)
// =========================================================

// Obtener la cadena de conexión de appsettings.json
var connectionString = builder.Configuration.GetConnectionString("GimpassDBConnection") ??
    throw new InvalidOperationException("Connection string 'GimpassDBConnection' not found.");

// Registrar el ApplicationDbContext con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


// =========================================================
// 2. REGISTRO DE SERVICIOS ADICIONALES (Controladores y Servicios de Lógica)
// =========================================================

// Añadir servicios para controladores y vistas
builder.Services.AddControllersWithViews();

// Registrar el servicio de IA para poder inyectarlo en DatosController
// Usamos AddScoped, lo cual es apropiado para servicios que manejan lógica de negocio por solicitud.
builder.Services.AddScoped<IAGymService>();


var app = builder.Build();

// =========================================================
// 3. CONFIGURACIÓN DEL PIPELINE HTTP (Middleware)
// =========================================================

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Mapeo de rutas (incluyendo tus rutas API)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();