using CasaVintage.Data;
using CasaVintage.Models;
using CasaVintage.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Licencia Community de QuestPDF (gratuita para este uso). Debe fijarse antes de generar PDFs.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages. Por convencion se exige autenticacion en TODAS las paginas y solo se permite
// el acceso anonimo a login, error y acceso denegado. Asi la autorizacion es real en servidor.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Cuenta/Login");
    options.Conventions.AllowAnonymousToPage("/Cuenta/Denegado");
    options.Conventions.AllowAnonymousToPage("/Error");
});

// Contexto de EF Core apuntando a la instancia local CASTWINGS\SQLEXPRESS (base CASA_VINTAGE).
builder.Services.AddDbContext<CasaVintageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CasaVintage")));

// Hasher de contrasenas (IPasswordHasher del framework); se usa en el seed y en la autenticacion.
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Servicio de autenticacion (logica de negocio del login).
builder.Services.AddScoped<IAuthService, AuthService>();

// Servicio del modulo de Usuarios / "Personal de la empresa" (solo Administrador).
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Almacenamiento de imagenes en disco (fotos de perfil ahora; fotos de productos mas adelante).
builder.Services.AddScoped<IAlmacenArchivos, AlmacenArchivos>();

// Servicio del modulo de Proveedores (Admin/Gerente).
builder.Services.AddScoped<IProveedorService, ProveedorService>();

// Servicio del modulo de Inventario/Productos (Admin/Gerente).
builder.Services.AddScoped<IProductoService, ProductoService>();

// Sesion en memoria para el carrito del vendedor (vive mientras dura la sesion del navegador).
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.Name = "CasaVintage.Carrito";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Servicio del carrito (guardado en la sesion) para el flujo de ventas.
builder.Services.AddScoped<ICarritoService, CarritoService>();

// Servicio de ventas: procesa la venta en una transaccion con concurrencia optimista.
builder.Services.AddScoped<IVentaService, VentaService>();

// Facturacion: comprobante en PDF (QuestPDF) y envio por correo (SMTP).
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFacturaService, FacturaService>();

// Reportes del Contador (resumen, mas/menos vendidos, por vendedor, historial).
builder.Services.AddScoped<IReporteService, ReporteService>();

// Guarda facturas y reportes en carpetas del disco (Rutas de appsettings).
builder.Services.AddScoped<IArchivadorLocal, ArchivadorLocal>();

// Logo de la empresa (identidad visual): lo cambia el Administrador desde Ajustes.
builder.Services.AddScoped<IMarcaService, MarcaService>();

// Autenticacion por cookies nativa (sin ASP.NET Identity completo).
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.LogoutPath = "/Cuenta/Logout";
        options.AccessDeniedPath = "/Cuenta/Denegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "CasaVintage.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Aplica migraciones pendientes (crea CASA_VINTAGE si no existe) y siembra datos de prueba.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<CasaVintageContext>();
    db.Database.Migrate();
    var hasher = services.GetRequiredService<IPasswordHasher<Usuario>>();
    await DbInitializer.SeedAsync(db, hasher);

    // Crea las carpetas de Facturas y Reportes al arrancar y registra su ruta absoluta,
    // asi el usuario ve donde se guardan (y confirma que corre el build actual).
    var archivador = services.GetRequiredService<IArchivadorLocal>();
    var log = services.GetRequiredService<ILogger<Program>>();
    log.LogInformation("Carpeta de Facturas: {Ruta}", archivador.AsegurarCarpeta("Facturas"));
    log.LogInformation("Carpeta de Reportes: {Ruta}", archivador.AsegurarCarpeta("Reportes"));
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// La sesion (carrito) debe estar disponible antes de ejecutar las paginas.
app.UseSession();

// El orden importa: primero autenticar (leer la cookie), luego autorizar.
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
