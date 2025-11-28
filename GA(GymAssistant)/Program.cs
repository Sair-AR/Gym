using GA_GymAssistant.Data;
using Microsoft.EntityFrameworkCore;
// Asegúrate de tener este using para tu servicio
using GA_GymAssistant.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Base de Datos
var connectionString = builder.Configuration.GetConnectionString("GimpassDBConnection")
    ?? throw new InvalidOperationException("Connection string 'GimpassDBConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. REGISTRO DEL SERVICIO DE IA (AQUÍ ESTÁ LA CORRECCIÓN)
// Usamos AddHttpClient para que funcione el constructor (HttpClient httpClient)
builder.Services.AddHttpClient<IAGymService>();

// 3. Controladores y Vistas
builder.Services.AddControllersWithViews();

// 4. CORS
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

// ... (Resto de la configuración del pipeline) ...

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();