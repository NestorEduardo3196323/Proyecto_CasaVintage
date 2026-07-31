using CasaVintage.Data;
using CasaVintage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CasaVintage.Services
{
    // Siembra datos iniciales de prueba: un usuario por cada rol (contrasena hasheada), proveedores
    // y productos. Es idempotente: los usuarios se insertan si no existe su correo, y proveedores/
    // productos solo cuando su tabla esta vacia. Puede ejecutarse en cada arranque sin duplicar.
    //
    // CREDENCIALES DE PRUEBA (cambiar en produccion). Contrasenas guardadas hasheadas:
    //   Administrador -> admin@casavintage.local     / Admin123*
    //   Gerente       -> gerente@casavintage.local   / Gerente123*
    //   Vendedor      -> vendedor@casavintage.local  / Vendedor123*
    //   Contador      -> contador@casavintage.local  / Contador123*
    public static class DbInitializer
    {
        // Definicion de un usuario de prueba (correo, contrasena en claro solo para el seed inicial).
        private sealed record UsuarioSemilla(string Nombre, string Correo, string Rol, string Password);

        // Un usuario por rol. La contrasena se hashea antes de guardar; nunca se persiste en claro.
        private static readonly UsuarioSemilla[] UsuariosPrueba =
        {
            new("Administrador General", "admin@casavintage.local", "Administrador", "Admin123*"),
            new("Gerente de Tienda", "gerente@casavintage.local", "Gerente", "Gerente123*"),
            new("Vendedor de Mostrador", "vendedor@casavintage.local", "Vendedor", "Vendedor123*"),
            new("Contador de la Empresa", "contador@casavintage.local", "Contador", "Contador123*")
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
            // Correos ya existentes, para no duplicar al reejecutar el seed.
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
                // El hash se calcula con IPasswordHasher; nunca se guarda la contrasena en texto plano.
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
            // Solo se siembra cuando la tabla esta VACIA (base nueva). Asi, si el usuario elimina un
            // producto, NO se vuelve a insertar al reiniciar: la eliminacion es permanente.
            if (await db.Productos.AnyAsync())
            {
                return;
            }

            // Se asocian a proveedores ya sembrados (por orden de insercion).
            var proveedores = await db.Proveedores.OrderBy(p => p.IdProveedor).ToListAsync();
            if (proveedores.Count == 0)
            {
                return;
            }

            // Imagenes ilustradas (SVG) que viven en wwwroot/uploads/productos. Se pueden reemplazar
            // por fotos reales desde el modulo de Inventario.
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

        // Construye un producto de prueba y sincroniza disponibilidad = (stock > 0).
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
