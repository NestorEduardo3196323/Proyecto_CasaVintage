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
            new("Administrador General", "admin@casavintage.local", "Administrador", "CasaVintage#Admin2026!"),
            new("Gerente de Tienda", "gerente@casavintage.local", "Gerente", "CasaVintage#Gerente2026!"),
            new("Vendedor de Mostrador", "vendedor@casavintage.local", "Vendedor", "CasaVintage#Vendedor2026!"),
            new("Contador de la Empresa", "contador@casavintage.local", "Contador", "CasaVintage#Contador2026!")
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
                new Proveedor { Nombre = "Antiguedades del Valle", Contacto = "Maria Reyes", Telefono = "2211-3344" },
                new Proveedor { Nombre = "Reliquias Lourdes", Contacto = "Jose Menjivar", Telefono = "2255-6677" },
                new Proveedor { Nombre = "Herencia Colonial", Contacto = "Ana Portillo", Telefono = "2299-1010" });

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
                Crear("VIN-0001", "Reloj de pared Art Decó", "Reloj de pared en madera de nogal con detalles dorados, mecanismo funcional.",
                    "Art Decó", "Bueno", 185.00m, 90.00m, 3, "Relojes", proveedores[0].IdProveedor),
                Crear("VIN-0002", "Máquina de escribir Underwood", "Máquina de escribir de colección, teclas restauradas, incluye estuche.",
                    "Años 20", "Restaurado", 240.00m, 130.00m, 1, "Coleccionables", proveedores[1].IdProveedor),
                Crear("VIN-0003", "Espejo estilo Victoriano", "Espejo ovalado con marco tallado a mano y baño de oro.",
                    "Victoriana", "Excelente", 320.00m, 175.00m, 2, "Decoración", proveedores[2].IdProveedor),
                Crear("VIN-0004", "Radio de bulbos Philco", "Radio de bulbos de los años 40, gabinete de madera, enciende y sintoniza.",
                    "Años 40", "Bueno", 210.00m, 115.00m, 0, "Electrónicos", proveedores[0].IdProveedor),
                Crear("VIN-0005", "Silla Luis XV tapizada", "Silla de madera tallada con tapiz floral restaurado, patas cabriolé firmes.",
                    "Victoriana", "Restaurado", 380.00m, 200.00m, 2, "Muebles", proveedores[0].IdProveedor, Fotos("silla")),
                Crear("VIN-0006", "Lámpara de mesa Tiffany", "Lámpara con pantalla de vitral emplomado en tonos ámbar, base de bronce.",
                    "Art Nouveau", "Excelente", 295.00m, 150.00m, 3, "Iluminación", proveedores[1].IdProveedor, Fotos("lampara")),
                Crear("VIN-0007", "Baúl de viaje de cuero", "Baúl de madera forrado en cuero con herrajes de latón y correas originales.",
                    "Años 30", "Bueno", 260.00m, 130.00m, 1, "Muebles", proveedores[2].IdProveedor, Fotos("baul")),
                Crear("VIN-0008", "Teléfono de disco", "Teléfono de disco giratorio en baquelita, cableado revisado y funcional.",
                    "Años 50", "Restaurado", 120.00m, 55.00m, 4, "Coleccionables", proveedores[0].IdProveedor, Fotos("telefono")),
                Crear("VIN-0009", "Cámara de fuelle", "Cámara fotográfica de fuelle con lente de bronce y estuche de cuero.",
                    "Años 20", "Bueno", 340.00m, 180.00m, 1, "Coleccionables", proveedores[1].IdProveedor, Fotos("camara")),
                Crear("VIN-0010", "Jarrón de porcelana china", "Jarrón de porcelana pintado a mano con motivos florales, sin fisuras.",
                    "Antigua", "Excelente", 220.00m, 110.00m, 2, "Decoración", proveedores[2].IdProveedor, Fotos("jarron")),
                Crear("VIN-0011", "Tocadiscos de maleta", "Tocadiscos portátil tipo maleta, aguja nueva, reproduce a 33 y 45 RPM.",
                    "Años 60", "Bueno", 175.00m, 85.00m, 2, "Electrónicos", proveedores[0].IdProveedor, Fotos("tocadiscos")),
                Crear("VIN-0012", "Candelabro de bronce", "Candelabro de tres brazos en bronce macizo con pátina original.",
                    "Victoriana", "Bueno", 145.00m, 70.00m, 3, "Iluminación", proveedores[1].IdProveedor, Fotos("candelabro"))
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
