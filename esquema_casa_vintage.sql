-- ============================================================
-- Esquema de referencia — Sistema de Inventario y Ventas
-- "La Casa de Vintage"   |   Base de datos: CASA_VINTAGE (SQL Server)
--
-- Refleja el esquema ACTUAL de la base tal como lo generan las
-- entidades y las migraciones de Entity Framework Core (code-first).
-- Es solo documentacion: la base la crean las migraciones, este
-- script NO se ejecuta a mano.
-- ============================================================

CREATE TABLE proveedores (
    id_proveedor INT PRIMARY KEY IDENTITY(1,1),
    nombre       VARCHAR(60)  NOT NULL,
    contacto     VARCHAR(100),
    telefono     VARCHAR(15)
);

CREATE TABLE clientes (
    id_cliente INT PRIMARY KEY IDENTITY(1,1),
    nombre     VARCHAR(100) NOT NULL,
    telefono   VARCHAR(15),
    correo     VARCHAR(100)
);

CREATE TABLE usuarios (
    id_usuario     INT PRIMARY KEY IDENTITY(1,1),
    nombre_usuario VARCHAR(60)  NOT NULL,                    -- nombre completo del empleado
    correo         VARCHAR(150) NOT NULL,                    -- login por correo
    password       VARCHAR(255) NOT NULL,                    -- hash (IPasswordHasher), nunca texto plano
    rol            VARCHAR(20)  NOT NULL,                    -- Administrador | Gerente | Vendedor | Contador
    activo         BIT          NOT NULL DEFAULT 1,          -- activar / desactivar cuenta
    fecha_creado   DATETIME     NOT NULL DEFAULT GETDATE(),  -- auditoria
    foto           VARCHAR(255),                             -- ruta de la foto de perfil en disco (no el binario)
    CONSTRAINT UQ_usuarios_correo UNIQUE (correo),
    CONSTRAINT CK_usuarios_rol CHECK (rol IN ('Administrador','Gerente','Vendedor','Contador'))
);

CREATE TABLE productos (
    id_producto    INT PRIMARY KEY IDENTITY(1,1),
    sku            VARCHAR(30)   NOT NULL,                   -- codigo unico autogenerado (VIN-####)
    nombre         VARCHAR(100)  NOT NULL,
    descripcion    VARCHAR(MAX)  NOT NULL,
    epoca          VARCHAR(50)   NOT NULL,
    estado         VARCHAR(50)   NOT NULL,                   -- estado de conservacion
    precio         DECIMAL(10,2) NOT NULL,                   -- precio de venta
    costo          DECIMAL(10,2) NOT NULL,                   -- costo de adquisicion (para rentabilidad)
    stock          INT           NOT NULL,
    stock_minimo   INT           NOT NULL DEFAULT 0,         -- nivel minimo para alerta de stock bajo (0 = sin alerta)
    disponibilidad BIT           NOT NULL,                   -- se sincroniza: disponibilidad = (stock > 0)
    categoria      VARCHAR(50)   NOT NULL,
    id_proveedor   INT           NOT NULL,
    foto1          VARCHAR(255),                             -- ruta del archivo en disco (no el binario)
    foto2          VARCHAR(255),
    foto3          VARCHAR(255),
    fecha_registro DATETIME      NOT NULL DEFAULT GETDATE(), -- auditoria
    row_version    ROWVERSION    NOT NULL,                   -- concurrencia optimista (EF Core)
    CONSTRAINT UQ_productos_sku UNIQUE (sku),
    CONSTRAINT FK_productos_proveedor FOREIGN KEY (id_proveedor) REFERENCES proveedores(id_proveedor),
    CONSTRAINT CK_productos_stock  CHECK (stock  >= 0),
    CONSTRAINT CK_productos_precio CHECK (precio >= 0),
    CONSTRAINT CK_productos_costo  CHECK (costo  >= 0)
);

CREATE TABLE ventas (
    id_venta     INT PRIMARY KEY IDENTITY(1,1),
    fecha        DATETIME      NOT NULL DEFAULT GETDATE(),
    id_cliente   INT           NOT NULL,
    id_usuario   INT           NOT NULL,
    total_pagado DECIMAL(10,2) NOT NULL,
    metodo_pago  VARCHAR(20)   NOT NULL,                     -- Efectivo | Tarjeta
    tarjeta_ultimos4 VARCHAR(4),                             -- solo los ultimos 4 digitos si el pago fue con tarjeta (nunca el numero completo ni el CVV)
    CONSTRAINT CK_ventas_metodo CHECK (metodo_pago IN ('Efectivo','Tarjeta')),
    CONSTRAINT FK_ventas_cliente FOREIGN KEY (id_cliente) REFERENCES clientes(id_cliente),
    CONSTRAINT FK_ventas_usuario FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);

CREATE TABLE detalle_venta (
    id_detalle      INT PRIMARY KEY IDENTITY(1,1),
    id_venta        INT           NOT NULL,
    id_producto     INT           NOT NULL,
    cantidad        INT           NOT NULL,
    precio_unitario DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_detalle_venta FOREIGN KEY (id_venta) REFERENCES ventas(id_venta),
    CONSTRAINT FK_detalle_producto FOREIGN KEY (id_producto) REFERENCES productos(id_producto),
    CONSTRAINT CK_detalle_cantidad CHECK (cantidad > 0)
);
