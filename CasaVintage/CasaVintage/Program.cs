using CasaVintage.Data;
using CasaVintage.Models;
using CasaVintage.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// QuestPDF Community license (free for this use). It must be set before generating PDFs.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages. By convention, authentication is required on ALL pages and anonymous access is only
// allowed to login, error and access denied. This way authorization is really enforced on the server.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Cuenta/Login");
    options.Conventions.AllowAnonymousToPage("/Cuenta/Denegado");
    options.Conventions.AllowAnonymousToPage("/Error");
});

// EF Core context pointing to the local instance CASTWINGS\SQLEXPRESS (database CASA_VINTAGE).
builder.Services.AddDbContext<CasaVintageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CasaVintage")));

// Password hasher (the framework's IPasswordHasher); used in the seed and in authentication.
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Authentication service (login business logic).
builder.Services.AddScoped<IAuthService, AuthService>();

// Users / "Company personnel" module service (Administrator only).
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Image storage on disk (profile photos now; product photos later on).
builder.Services.AddScoped<IAlmacenArchivos, AlmacenArchivos>();

// Suppliers module service (Admin/Manager).
builder.Services.AddScoped<IProveedorService, ProveedorService>();

// Inventory/Products module service (Admin/Manager).
builder.Services.AddScoped<IProductoService, ProductoService>();

// In-memory session for the salesperson's cart (it lives as long as the browser session lasts).
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.Name = "CasaVintage.Carrito";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cart service (stored in the session) for the sales flow.
builder.Services.AddScoped<ICarritoService, CarritoService>();

// Sales service: processes the sale in a transaction with optimistic concurrency.
builder.Services.AddScoped<IVentaService, VentaService>();

// Billing: PDF receipt (QuestPDF) and sending by email (SMTP).
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFacturaService, FacturaService>();

// Accountant reports (summary, best/least sold, by salesperson, history).
builder.Services.AddScoped<IReporteService, ReporteService>();

// Saves invoices and reports into folders on disk (Paths from appsettings).
builder.Services.AddScoped<IArchivadorLocal, ArchivadorLocal>();

// Company logo (visual identity): the Administrator changes it from Settings.
builder.Services.AddScoped<IMarcaService, MarcaService>();

// Native cookie authentication (without the full ASP.NET Identity).
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

// Applies pending migrations (creates CASA_VINTAGE if it does not exist) and seeds test data.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<CasaVintageContext>();
    db.Database.Migrate();
    var hasher = services.GetRequiredService<IPasswordHasher<Usuario>>();
    await DbInitializer.SeedAsync(db, hasher);

    // Creates the Invoices and Reports folders at startup and logs their absolute path,
    // so the user sees where they are saved (and confirms the current build is running).
    var archivador = services.GetRequiredService<IArchivadorLocal>();
    var log = services.GetRequiredService<ILogger<Program>>();
    log.LogInformation("Invoices folder: {Ruta}", archivador.AsegurarCarpeta("Facturas"));
    log.LogInformation("Reports folder: {Ruta}", archivador.AsegurarCarpeta("Reportes"));
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

// The session (cart) must be available before executing the pages.
app.UseSession();

// The order matters: first authenticate (read the cookie), then authorize.
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
