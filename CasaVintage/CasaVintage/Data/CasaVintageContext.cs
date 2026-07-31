using CasaVintage.Models;
using Microsoft.EntityFrameworkCore;

namespace CasaVintage.Data
{
    // Contexto de EF Core. Es la fuente de verdad del esquema: las entidades se mapean
    // exactamente a las tablas y columnas de docs/esquema_casa_vintage.sql (snake_case, varchar,
    // checks, defaults y row_version) para que las migraciones generen la base CASA_VINTAGE.
    public class CasaVintageContext : DbContext
    {
        public CasaVintageContext(DbContextOptions<CasaVintageContext> options) : base(options)
        {
        }

        public DbSet<Proveedor> Proveedores => Set<Proveedor>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------------- proveedores ----------------
            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.ToTable("proveedores");
                entity.HasKey(e => e.IdProveedor);
                entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasColumnType("varchar(60)").IsRequired();
                entity.Property(e => e.Contacto).HasColumnName("contacto").HasColumnType("varchar(100)");
                entity.Property(e => e.Telefono).HasColumnName("telefono").HasColumnType("varchar(15)");
            });

            // ---------------- clientes ----------------
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("clientes");
                entity.HasKey(e => e.IdCliente);
                entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasColumnType("varchar(100)").IsRequired();
                entity.Property(e => e.Telefono).HasColumnName("telefono").HasColumnType("varchar(15)");
                entity.Property(e => e.Correo).HasColumnName("correo").HasColumnType("varchar(100)");
            });

            // ---------------- usuarios ----------------
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuarios", t =>
                    t.HasCheckConstraint("CK_usuarios_rol",
                        "rol IN ('Administrador','Gerente','Vendedor','Contador')"));
                entity.HasKey(e => e.IdUsuario);
                entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
                entity.Property(e => e.NombreUsuario).HasColumnName("nombre_usuario").HasColumnType("varchar(60)").IsRequired();
                entity.Property(e => e.Correo).HasColumnName("correo").HasColumnType("varchar(150)").IsRequired();
                entity.Property(e => e.Password).HasColumnName("password").HasColumnType("varchar(255)").IsRequired();
                entity.Property(e => e.Rol).HasColumnName("rol").HasColumnType("varchar(20)").IsRequired();
                entity.Property(e => e.Activo).HasColumnName("activo").HasDefaultValue(true);
                entity.Property(e => e.FechaCreado).HasColumnName("fecha_creado").HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Foto).HasColumnName("foto").HasColumnType("varchar(255)");
                entity.HasIndex(e => e.Correo).IsUnique().HasDatabaseName("UQ_usuarios_correo");
            });

            // ---------------- productos ----------------
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("productos", t =>
                {
                    t.HasCheckConstraint("CK_productos_stock", "stock >= 0");
                    t.HasCheckConstraint("CK_productos_precio", "precio >= 0");
                    t.HasCheckConstraint("CK_productos_costo", "costo >= 0");
                });
                entity.HasKey(e => e.IdProducto);
                entity.Property(e => e.IdProducto).HasColumnName("id_producto");
                entity.Property(e => e.Sku).HasColumnName("sku").HasColumnType("varchar(30)").IsRequired();
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasColumnType("varchar(100)").IsRequired();
                entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasColumnType("varchar(max)").IsRequired();
                entity.Property(e => e.Epoca).HasColumnName("epoca").HasColumnType("varchar(50)").IsRequired();
                entity.Property(e => e.Estado).HasColumnName("estado").HasColumnType("varchar(50)").IsRequired();
                entity.Property(e => e.Precio).HasColumnName("precio").HasColumnType("decimal(10,2)");
                entity.Property(e => e.Costo).HasColumnName("costo").HasColumnType("decimal(10,2)");
                entity.Property(e => e.Stock).HasColumnName("stock");
                entity.Property(e => e.StockMinimo).HasColumnName("stock_minimo").HasDefaultValue(0);
                entity.Property(e => e.Disponibilidad).HasColumnName("disponibilidad");
                entity.Property(e => e.Categoria).HasColumnName("categoria").HasColumnType("varchar(50)").IsRequired();
                entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
                entity.Property(e => e.Foto1).HasColumnName("foto1").HasColumnType("varchar(255)");
                entity.Property(e => e.Foto2).HasColumnName("foto2").HasColumnType("varchar(255)");
                entity.Property(e => e.Foto3).HasColumnName("foto3").HasColumnType("varchar(255)");
                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro").HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.RowVersion).HasColumnName("row_version").IsRowVersion();
                entity.HasIndex(e => e.Sku).IsUnique().HasDatabaseName("UQ_productos_sku");
                entity.HasOne(e => e.Proveedor)
                    .WithMany(p => p.Productos)
                    .HasForeignKey(e => e.IdProveedor)
                    .HasConstraintName("FK_productos_proveedor")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------- ventas ----------------
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.ToTable("ventas", t =>
                    t.HasCheckConstraint("CK_ventas_metodo", "metodo_pago IN ('Efectivo','Tarjeta')"));
                entity.HasKey(e => e.IdVenta);
                entity.Property(e => e.IdVenta).HasColumnName("id_venta");
                entity.Property(e => e.Fecha).HasColumnName("fecha").HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
                entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
                entity.Property(e => e.TotalPagado).HasColumnName("total_pagado").HasColumnType("decimal(10,2)");
                entity.Property(e => e.MetodoPago).HasColumnName("metodo_pago").HasColumnType("varchar(20)").IsRequired();
                entity.Property(e => e.TarjetaUltimos4).HasColumnName("tarjeta_ultimos4").HasColumnType("varchar(4)");
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Ventas)
                    .HasForeignKey(e => e.IdCliente)
                    .HasConstraintName("FK_ventas_cliente")
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Usuario)
                    .WithMany(u => u.Ventas)
                    .HasForeignKey(e => e.IdUsuario)
                    .HasConstraintName("FK_ventas_usuario")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------- detalle_venta ----------------
            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                entity.ToTable("detalle_venta", t =>
                    t.HasCheckConstraint("CK_detalle_cantidad", "cantidad > 0"));
                entity.HasKey(e => e.IdDetalle);
                entity.Property(e => e.IdDetalle).HasColumnName("id_detalle");
                entity.Property(e => e.IdVenta).HasColumnName("id_venta");
                entity.Property(e => e.IdProducto).HasColumnName("id_producto");
                entity.Property(e => e.Cantidad).HasColumnName("cantidad");
                entity.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("decimal(10,2)");
                entity.HasOne(e => e.Venta)
                    .WithMany(v => v.Detalles)
                    .HasForeignKey(e => e.IdVenta)
                    .HasConstraintName("FK_detalle_venta")
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Producto)
                    .WithMany(p => p.Detalles)
                    .HasForeignKey(e => e.IdProducto)
                    .HasConstraintName("FK_detalle_producto")
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
