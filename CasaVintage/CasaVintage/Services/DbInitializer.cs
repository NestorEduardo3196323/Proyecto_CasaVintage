using CasaVintage.Data;
using CasaVintage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CasaVintage.Services
{
    // Seeds initial test data: one user per role (hashed password), suppliers and products. It is
    // idempotent: users are inserted if their email does not exist, and suppliers/products only when
    // their table is empty. It can run on every startup without duplicating.
    //
    // TEST CREDENTIALS (change in production). Passwords stored hashed:
    //   Administrador -> admin@casavintage.local     / CasaVintage#Admin2026!
    //   Gerente       -> gerente@casavintage.local   / CasaVintage#Gerente2026!
    //   Vendedor      -> vendedor@casavintage.local  / CasaVintage#Vendedor2026!
    //   Contador      -> contador@casavintage.local  / CasaVintage#Contador2026!
    public static class DbInitializer
    {
        // Definition of a test user (email, plain password only for the initial seed).
        private sealed record UsuarioSemilla(string Nombre, string Correo, string Rol, string Password);

        // One user per role. The password is hashed before saving; it is never persisted in plain text.
        private static readonly UsuarioSemilla[] UsuariosPrueba =
        {
            new("General Administrator", "admin@casavintage.local", "Administrador", "CasaVintage#Admin2026!"),
            new("Store Manager", "gerente@casavintage.local", "Gerente", "CasaVintage#Gerente2026!"),
            new("Counter Salesperson", "vendedor@casavintage.local", "Vendedor", "CasaVintage#Vendedor2026!"),
            new("Company Accountant", "contador@casavintage.local", "Contador", "CasaVintage#Contador2026!")
        };

        public static async Task SeedAsync(CasaVintageContext db, IPasswordHasher<Usuario> hasher)
        {
            await SeedProveedoresAsync(db);
            await SeedUsuariosAsync(db, hasher);
            await SeedProductosAsync(db);
        }

        private static async Task SeedProveedoresAsync(CasaVintageContext db)
        {
            if (await db.Proveedores.AnyAsync())
            {
                return;
            }

            db.Proveedores.AddRange(
                new Proveedor { Nombre = "Valley Antiques", Contacto = "Maria Reyes", Telefono = "2211-3344" },
                new Proveedor { Nombre = "Lourdes Relics", Contacto = "Jose Menjivar", Telefono = "2255-6677" },
                new Proveedor { Nombre = "Colonial Heritage", Contacto = "Ana Portillo", Telefono = "2299-1010" });

            await db.SaveChangesAsync();
        }

        private static async Task SeedUsuariosAsync(CasaVintageContext db, IPasswordHasher<Usuario> hasher)
        {
            // Already-existing emails, so as not to duplicate when re-running the seed.
            var existentes = await db.Usuarios.Select(u => u.Correo).ToListAsync();

            var nuevos = false;
            foreach (var semilla in UsuariosPrueba)
            {
                if (existentes.Contains(semilla.Correo))
                {
                    continue;
                }

                var usuario = new Usuario
                {
                    NombreUsuario = semilla.Nombre,
                    Correo = semilla.Correo,
                    Rol = semilla.Rol,
                    Activo = true
                };
                // The hash is computed with IPasswordHasher; the password is never stored in plain text.
                usuario.Password = hasher.HashPassword(usuario, semilla.Password);

                db.Usuarios.Add(usuario);
                nuevos = true;
            }

            if (nuevos)
            {
                await db.SaveChangesAsync();
            }
        }

        private static async Task SeedProductosAsync(CasaVintageContext db)
        {
            // It is only seeded when the table is EMPTY (new database). This way, if the user deletes a
            // product, it is NOT re-inserted on restart: the deletion is permanent.
            if (await db.Productos.AnyAsync())
            {
                return;
            }

            // They are associated with already-seeded suppliers (by insertion order).
            var proveedores = await db.Proveedores.OrderBy(p => p.IdProveedor).ToListAsync();
            if (proveedores.Count == 0)
            {
                return;
            }

            // Illustrated images (SVG) that live in wwwroot/uploads/productos. They can be replaced
            // by real photos from the Inventory module.
            static string[] Fotos(string slug) => new[]
            {
                $"/uploads/productos/seed-{slug}-1.svg",
                $"/uploads/productos/seed-{slug}-2.svg",
                $"/uploads/productos/seed-{slug}-3.svg"
            };

            var catalogo = new List<Producto>
            {
                Crear("VIN-0001", "Art Deco Wall Clock", "Walnut wood wall clock with gilded details, working mechanism.",
                    "Art Deco", "Good", 185.00m, 90.00m, 3, "Clocks", proveedores[0].IdProveedor),
                Crear("VIN-0002", "Underwood Typewriter", "Collector's typewriter, restored keys, includes case.",
                    "1920s", "Restored", 240.00m, 130.00m, 1, "Collectibles", proveedores[1].IdProveedor),
                Crear("VIN-0003", "Victorian-Style Mirror", "Oval mirror with a hand-carved frame and a gold finish.",
                    "Victorian", "Excellent", 320.00m, 175.00m, 2, "Decor", proveedores[2].IdProveedor),
                Crear("VIN-0004", "Philco Tube Radio", "1940s tube radio, wooden cabinet, powers on and tunes.",
                    "1940s", "Good", 210.00m, 115.00m, 0, "Audio & Electronics", proveedores[0].IdProveedor),
                Crear("VIN-0005", "Upholstered Louis XV Chair", "Carved wood chair with restored floral upholstery and firm cabriole legs.",
                    "Victorian", "Restored", 380.00m, 200.00m, 2, "Furniture", proveedores[0].IdProveedor, Fotos("silla")),
                Crear("VIN-0006", "Tiffany Table Lamp", "Lamp with a leaded stained-glass shade in amber tones and a bronze base.",
                    "Art Nouveau", "Excellent", 295.00m, 150.00m, 3, "Lighting", proveedores[1].IdProveedor, Fotos("lampara")),
                Crear("VIN-0007", "Leather Travel Trunk", "Wooden trunk lined in leather with brass fittings and original straps.",
                    "1930s", "Good", 260.00m, 130.00m, 1, "Furniture", proveedores[2].IdProveedor, Fotos("baul")),
                Crear("VIN-0008", "Rotary Dial Telephone", "Rotary dial telephone in bakelite, wiring checked and working.",
                    "1950s", "Restored", 120.00m, 55.00m, 4, "Collectibles", proveedores[0].IdProveedor, Fotos("telefono")),
                Crear("VIN-0009", "Bellows Camera", "Bellows photographic camera with a bronze lens and a leather case.",
                    "1920s", "Good", 340.00m, 180.00m, 1, "Collectibles", proveedores[1].IdProveedor, Fotos("camara")),
                Crear("VIN-0010", "Chinese Porcelain Vase", "Hand-painted porcelain vase with floral motifs, no cracks.",
                    "Antique", "Excellent", 220.00m, 110.00m, 2, "Decor", proveedores[2].IdProveedor, Fotos("jarron")),
                Crear("VIN-0011", "Suitcase Turntable", "Portable suitcase-style turntable, new needle, plays at 33 and 45 RPM.",
                    "1960s", "Good", 175.00m, 85.00m, 2, "Audio & Electronics", proveedores[0].IdProveedor, Fotos("tocadiscos")),
                Crear("VIN-0012", "Bronze Candelabra", "Three-arm candelabra in solid bronze with its original patina.",
                    "Victorian", "Good", 145.00m, 70.00m, 3, "Lighting", proveedores[1].IdProveedor, Fotos("candelabro"))
            };

            db.Productos.AddRange(catalogo);
            await db.SaveChangesAsync();
        }

        // Builds a test product and syncs availability = (stock > 0).
        private static Producto Crear(string sku, string nombre, string descripcion, string epoca,
            string estado, decimal precio, decimal costo, int stock, string categoria, int idProveedor, string[]? fotos = null)
        {
            return new Producto
            {
                Sku = sku,
                Nombre = nombre,
                Descripcion = descripcion,
                Epoca = epoca,
                Estado = estado,
                Precio = precio,
                Costo = costo,
                Stock = stock,
                Disponibilidad = stock > 0,
                Categoria = categoria,
                IdProveedor = idProveedor,
                Foto1 = fotos is { Length: > 0 } ? fotos[0] : null,
                Foto2 = fotos is { Length: > 1 } ? fotos[1] : null,
                Foto3 = fotos is { Length: > 2 } ? fotos[2] : null
            };
        }
    }
}
