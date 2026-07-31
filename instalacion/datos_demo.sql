-- =====================================================================
-- datos_demo.sql  -  Datos de demostracion  -  Sistema La Casa de Vintage
-- Base de datos: CASA_VINTAGE  (SQL Server)
--
-- Proposito: cargar datos de prueba (proveedores, usuarios, productos y
-- una venta de ejemplo) para que el sistema se vea funcionando y no vacio.
--
-- La aplicacion tambien carga estos datos automaticamente la primera vez
-- que arranca (sembrado inicial). Este script sirve para cargarlos o
-- recargarlos a mano sobre una base ya creada por las migraciones.
--
-- Las contrasenas NUNCA se guardan en texto plano: la columna "password"
-- contiene el hash real generado por el componente de hashing del sistema.
-- Credenciales de acceso (usuario / contrasena):
--   Administrador : admin@casavintage.local     / Admin123*
--   Gerente       : gerente@casavintage.local   / Gerente123*
--   Vendedor      : vendedor@casavintage.local  / Vendedor123*
--   Contador      : contador@casavintage.local  / Contador123*
--
-- Ejecucion:
--   - SQL Server Management Studio: abrir este archivo y ejecutar (F5); o
--   - Linea de comandos:
--       sqlcmd -S CASTWINGS\SQLEXPRESS -d CASA_VINTAGE -E -i datos_demo.sql
--   Guardar/abrir este archivo con codificacion UTF-8.
--
-- Requiere que la base CASA_VINTAGE ya exista (creada con las migraciones:
--   dotnet ef database update --project CasaVintage/CasaVintage).
-- =====================================================================

USE CASA_VINTAGE;
GO

-- ---------------------------------------------------------------------
-- (OPCIONAL) Recarga limpia. Descomentar el bloque para borrar los datos
-- actuales antes de insertar (respeta el orden de las llaves foraneas).
-- Advertencia: elimina TODA la informacion de esas tablas.
-- ---------------------------------------------------------------------
-- DELETE FROM detalle_venta;
-- DELETE FROM ventas;
-- DELETE FROM productos;
-- DELETE FROM clientes;
-- DELETE FROM usuarios;
-- DELETE FROM proveedores;
-- GO

-- ---------------------------------------------------------------------
-- 1. Proveedores  (se insertan antes que los productos)
-- ---------------------------------------------------------------------
SET IDENTITY_INSERT proveedores ON;
INSERT INTO proveedores (id_proveedor, nombre, contacto, telefono) VALUES
 (1, N'Antiguedades del Valle', N'Maria Reyes',   N'2211-3344'),
 (2, N'Reliquias Lourdes',      N'Jose Menjivar', N'2255-6677'),
 (3, N'Herencia Colonial',      N'Ana Portillo',  N'2299-1010');
SET IDENTITY_INSERT proveedores OFF;
GO

-- ---------------------------------------------------------------------
-- 2. Usuarios  (uno por rol). La columna password guarda el HASH real.
-- ---------------------------------------------------------------------
SET IDENTITY_INSERT usuarios ON;
INSERT INTO usuarios (id_usuario, nombre_usuario, correo, password, rol, activo) VALUES
 (1, N'Administrador General', N'admin@casavintage.local',    'AQAAAAIAAYagAAAAENmBeVmlPF1Pusy6tsAUkZnXwWdHS+4MMogbyjgZbRShQcfcFG3t8MTQcVVWAX7WjQ==', N'Administrador', 1),
 (2, N'Gerente de Tienda',     N'gerente@casavintage.local',  'AQAAAAIAAYagAAAAED0rORgy0sj4bHBEB6e5kNk8Peo47gqEXuPbJr7ztCc4hUSPQEaNiLIJ9sAW6RINTg==', N'Gerente',       1),
 (3, N'Vendedor de Mostrador', N'vendedor@casavintage.local', 'AQAAAAIAAYagAAAAEFAojD2TGyEPR92yNIV/c83/bJko986zn0kn9sYDpu30PZIzXOQtJcO2FXcV/7uwfQ==', N'Vendedor',      1),
 (4, N'Contador de la Empresa',N'contador@casavintage.local', 'AQAAAAIAAYagAAAAELbXzFmoCQBrrgE2WzaQ4h612Gwvl3atuVlVfWjOLYbCd26ujR/t9HYQXC9sDwPt1g==', N'Contador',      1);
SET IDENTITY_INSERT usuarios OFF;
GO

-- ---------------------------------------------------------------------
-- 3. Productos. disponibilidad = 1 cuando stock > 0 (0 si esta agotado).
--    El SKU es unico. No se inserta row_version (la genera SQL Server).
-- ---------------------------------------------------------------------
SET IDENTITY_INSERT productos ON;
INSERT INTO productos (id_producto, sku, nombre, descripcion, epoca, estado, precio, costo, stock, stock_minimo, disponibilidad, categoria, id_proveedor) VALUES
 (1, N'VIN-0001', N'Reloj de pared Art Deco',       N'Reloj de pared en madera de nogal con detalles dorados, mecanismo funcional.', N'Art Deco',    N'Bueno',      185.00,  90.00, 3, 0, 1, N'Relojes',        1),
 (2, N'VIN-0002', N'Maquina de escribir Underwood',  N'Maquina de escribir de coleccion, teclas restauradas, incluye estuche.',       N'Anos 20',     N'Restaurado', 240.00, 130.00, 1, 0, 1, N'Coleccionables', 2),
 (3, N'VIN-0003', N'Espejo estilo Victoriano',       N'Espejo ovalado con marco tallado a mano y bano de oro.',                        N'Victoriana',  N'Excelente',  320.00, 175.00, 2, 0, 1, N'Decoracion',     3),
 (4, N'VIN-0004', N'Radio de bulbos Philco',         N'Radio de bulbos de los anos 40, gabinete de madera, enciende y sintoniza.',    N'Anos 40',     N'Bueno',      210.00, 115.00, 0, 0, 0, N'Electronicos',   1),
 (5, N'VIN-0005', N'Silla Luis XV tapizada',         N'Silla de madera tallada con tapiz floral restaurado, patas firmes.',           N'Victoriana',  N'Restaurado', 380.00, 200.00, 2, 0, 1, N'Muebles',        1),
 (6, N'VIN-0006', N'Lampara de mesa Tiffany',        N'Lampara con pantalla de vitral emplomado en tonos ambar, base de bronce.',     N'Art Nouveau', N'Excelente',  295.00, 150.00, 3, 0, 1, N'Iluminacion',    2),
 (7, N'VIN-0007', N'Baul de viaje de cuero',         N'Baul de madera forrado en cuero con herrajes de laton y correas originales.',  N'Anos 30',     N'Bueno',      260.00, 130.00, 1, 0, 1, N'Muebles',        3),
 (8, N'VIN-0008', N'Telefono de disco',              N'Telefono de disco giratorio en baquelita, cableado revisado y funcional.',     N'Anos 50',     N'Restaurado', 120.00,  55.00, 4, 0, 1, N'Coleccionables', 1);
SET IDENTITY_INSERT productos OFF;
GO

-- ---------------------------------------------------------------------
-- 4. (Opcional) Cliente y venta de ejemplo, para mostrar el historial y
--    los reportes con informacion. La venta la registra el Vendedor.
-- ---------------------------------------------------------------------
SET IDENTITY_INSERT clientes ON;
INSERT INTO clientes (id_cliente, nombre, telefono, correo) VALUES
 (1, N'Cliente de Ejemplo', N'7777-8888', N'cliente@ejemplo.com');
SET IDENTITY_INSERT clientes OFF;
GO

SET IDENTITY_INSERT ventas ON;
INSERT INTO ventas (id_venta, id_cliente, id_usuario, total_pagado, metodo_pago) VALUES
 (1, 1, 3, 120.00, N'Efectivo');
SET IDENTITY_INSERT ventas OFF;
GO

SET IDENTITY_INSERT detalle_venta ON;
INSERT INTO detalle_venta (id_detalle, id_venta, id_producto, cantidad, precio_unitario) VALUES
 (1, 1, 8, 1, 120.00);
SET IDENTITY_INSERT detalle_venta OFF;
GO

PRINT 'Datos de demostracion cargados correctamente.';
GO
