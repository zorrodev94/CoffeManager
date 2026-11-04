# Auditoría del avance Obsidiana

Fecha: 2026-11-03. Comparación contra el ZIP CoffeManager_Espejos_Formularios(1).

## Ejecutado

- `dotnet build CoffeManager.slnx --no-restore -m:1`: ambos proyectos correctos, 0 advertencias y 0 errores.
- `python3 Tests/check_mirrors.py`: cuatro parejas de entidades con estructuras coincidentes; Contracts sin estructuras de interacción.
- `dotnet run --project Tests/Transport/Transport.csproj`: respuestas 204, 200 vacío, JSON null, JSON válido y respuestas malformadas controladas.
- `dotnet run --project Tests/DomainChecks/DomainChecks.csproj`: serialización de perfiles, límites y cabeceras/tamaños de imágenes correctos. Integración SQL omitida expresamente por ausencia de instancia.
- API arrancada localmente: health 200; consultas anónimas privadas 401; solicitudes inválidas 400; activación sin configuración 503. No se crearon cuentas durante estas pruebas.
- Recurso PNG generado presente e incluido mediante la regla EmbeddedResource existente.
- Scripts SQL sin cambios; no se requiere una nueva migración.

## Revisión manual pendiente en Windows

1. API y cliente de esta misma entrega abiertos; conexión SQL configurada.
2. Con propietario existente: consulta de estado correcta, activación oculta, login existente funcional.
3. Con base de pruebas vacía y esquema 1/2: activación visible; campos obligatorios, clave incorrecta, confirmación distinta y registro correcto.
4. Intentar otra activación: el servidor debe continuar rechazándola, aunque se solicite por fuera del botón.
5. API apagada: mensaje de error y reintento mediante Comprobar conexión.
6. Teclado: Tab, Shift+Tab, Enter, Escape y mostrar/ocultar contraseña.
7. Ventana restaurada/maximizada, monitor pequeño, escalados 100/150/200 % y desplazamiento hasta los botones.
8. Inicio de sesión y cierre de Administración conservando el comportamiento existente de logout.

No se ejecutó WinForms en este entorno Linux. Tampoco se ejecutó el nuevo SELECT de estado contra SQL Server. Compilar no verifica la apariencia ni confirma inserciones reales. La verificación anterior que Christopher realizó en su equipo no sustituye estas pruebas de la entrega nueva.

## Alcance

Se agregó una consulta pública mínima de disponibilidad de activación; no devuelve nombres, correos, claves ni identificadores. Está limitada a 20 peticiones por minuto/IP con política independiente del login. Su resultado refleja el instante de la consulta. La transacción y el índice SQL existentes siguen siendo la autoridad para crear al primer dueño.

El método IsSetupRequiredAsync del cliente es de transporte. El SELECT vive en AccountRepository del servidor. Ninguna contraseña se almacena en el cliente como configuración y el transporte HTTPS existente sigue activo.

La cuenta inicial conserva su hash y el identificador de secretos permanece igual. No se cambiaron estructuras espejo, contraseñas existentes, esquemas ni asignaciones de empleados.

## Nota de diseño

El arte es una ilustración estática instalada con el cliente. La implementación usa controles WinForms y GDI+, sin animación, WebView ni motor 3D. Es una aproximación funcional al concepto Obsidiana, no una captura generada presentada como ejecución real.
