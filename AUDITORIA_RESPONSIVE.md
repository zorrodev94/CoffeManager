# Validación de Obsidiana · distribución y cuentas

## Ejecutado en este entorno

- Recompilación completa de la solución .NET 10: 0 advertencias, 0 errores.
- Cuatro parejas de estructuras espejo: coincidencia verificada; sin estructuras en Contracts.
- Tests/LayoutChecks: 18 combinaciones de viewport y DPI 96/144/192. Paneles dentro del área, sin solapamiento y fotografía presente. Son cálculos de geometría; no ejecutan controles WinForms.
- Tests/Transport: 204, cuerpo vacío, JSON null, JSON válido y respuestas malformadas controladas.
- Tests/DomainChecks: serialización, límites e imágenes; las seis combinaciones incompletas de empleado/sucursal/rol se rechazan antes de abrir SQL.
- API ejecutada: health 200, recursos privados anónimos 401, creación de staff sin sesión 401, solicitudes inválidas 400 y activación sin configuración 503.
- Scripts SQL sin cambios respecto de la entrega anterior.

## Pruebas preparadas, no ejecutadas

La regresión SQL opcional ahora incluye creación de cuenta vinculada y rechazo de un empleado que ya tiene cuenta, comprobando que no aparezca una cuenta residual. No se ejecutó porque no hay SQL Server aquí. No se afirma validada la transacción contra un motor real.

## Pendiente en Windows

- Apariencia y legibilidad de los controles: 4K/200 %, 1080p/100 % y laptop.
- Restaurar, maximizar y cambiar de monitor. Scroll vertical y ausencia de scroll horizontal.
- Primer arranque del menú y alternancia expandido/compacto.
- Cuenta de propietario existente, contraseña incorrecta y sesión.
- Alta de cuenta sin vinculación; alta con empleado+sucursal+rol; rechazo de correo duplicado; intento de vincular empleado ocupado.
- Rol nuevo con nombre español y capacidades explícitas. Cajero no entra a Administración si solo tiene acceso de Caja.

El ZIP contiene fuentes y recursos, sin bin/obj ni base de datos del usuario. No se alteraron ni recrearon los datos existentes.
