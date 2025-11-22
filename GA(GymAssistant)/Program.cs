using GA_GymAssistant.Data;
using GA_GymAssistant.Services;
using Microsoft.EntityFrameworkCore;

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
// 2. REGISTRO DE SERVICIOS ADICIONALES Y CORRECCIONES CRÍTICAS
// =========================================================

// Añadir servicios para controladores y vistas
builder.Services.AddControllersWithViews();

// 💡 CORRECCIÓN CRÍTICA: Usar AddHttpClient para registrar el servicio de IA.
// Esto resuelve el error 'Unable to resolve service for type System.Net.Http.HttpClient'
// al inyectar automáticamente el cliente HTTP necesario para llamar a Gemini.
builder.Services.AddHttpClient<IAGymService>();


// 💡 AÑADIDO: Configuración de CORS.
// Esto es esencial para que el frontend (index.html) pueda hacer peticiones 
// al backend (localhost:5000) sin ser bloqueado por el navegador.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowFrontend",
        policy =>
        {
            // Permitir cualquier origen (necesario para el entorno de desarrollo y pruebas locales)
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// =========================================================
// 3. CONFIGURACIÓN DEL PIPELINE HTTP (Middleware)
// =========================================================

// Configuración para el pipeline de peticiones HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 💡 APLICAR LA POLÍTICA DE CORS AQUÍ (debe ir después de UseRouting y antes de UseAuthorization)
app.UseCors("AllowFrontend");

app.UseAuthorization();

// Mapeo de rutas (incluyendo tus rutas API)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();