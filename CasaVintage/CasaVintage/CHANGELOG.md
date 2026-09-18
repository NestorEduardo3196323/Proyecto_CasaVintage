# Changelog — La Casa Vintage

Formato: MAYOR.MENOR.PARCHE (SemVer).

## [1.1.0] — 2026-09-10
### Agregado
- Columna `es_demo` en la tabla `clientes` para distinguir los registros de demostración de los datos reales futuros (mantenimiento preventivo, hallazgo F-08). Verificada en la copia de desarrollo y desplegada en producción (28 registros marcados).

### Cambiado (seguridad)
- Se reemplazaron las contraseñas por defecto débiles de las cuatro cuentas de rol (Administrador, Gerente, Vendedor, Contador) por contraseñas fuertes y únicas; siguen almacenándose solo como hash PBKDF2 (hallazgo F-05). Verificado iniciando sesión con la nueva y confirmando que la anterior es rechazada.

### Mantenimiento
- Primer respaldo verificado del sistema (código + base de datos), creado y **restaurado con éxito** en una base de prueba `CASA_VINTAGE_DEV` (F-01).
- Respaldos almacenados en dos carpetas del equipo **y en una unidad USB externa** (copia fuera del equipo) (F-02, F-03).

### Pospuesto al próximo mantenimiento (2026-09-18)
- Publicar el código en un repositorio Git remoto.
- Diagrama entidad–relación actualizado (agregar `es_demo`).

## [1.0.0] — anterior
- Versión inicial del sistema de inventario y ventas.
