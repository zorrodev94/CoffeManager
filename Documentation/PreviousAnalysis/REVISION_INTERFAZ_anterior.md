# Revisión de interfaz y estabilidad — 21 de octubre de 2026

Se conserva la paleta elegida por Alonso, el zorro y la fotografía de acceso originales. El nombre del tema no se muestra en las ventanas.

## Cambios verificables

- Navegación con iconos vectoriales locales: inicio, persona, negocio, sucursales, empleados y salida. Sin emojis ni descargas de iconos.
- Contracción inmediata a una barra de iconos de 72 unidades lógicas. El botón hamburguesa tiene las mismas medidas que los demás. Los nombres siguen disponibles como ayudas y nombres accesibles. No se introducen temporizadores, animaciones ni delays.
- Ajuste del ancho de la barra al cambiar DPI; las fichas y etiquetas se adaptan al espacio disponible.
- Fichas coherentes para perfil, negocio, sucursales y empleados. Avatar neutro local para el perfil sin fotografía.
- Los editores de administración y activación validan antes de cerrarse. Una entrada inválida conserva el resto del formulario. La activación comprueba también la coincidencia de contraseñas antes de confirmar.
- Negocio inexistente: la API devuelve 204; el cliente lo interpreta como ausencia y permite crear el negocio. También tolera una respuesta 200 vacía de la versión anterior.
- Una respuesta JSON inválida se convierte en un error controlado de conexión, en lugar de propagar la excepción de deserialización que aparece en la captura.

## Actualizar tu copia

1. Detén cliente y API. Extrae esta versión en una carpeta nueva, conservando tu versión anterior.
2. Abre `CoffeManager.slnx` y compila. Ejecuta la API y después el cliente administrativo como indica `INICIO_CLIENTE.md`.
3. Conserva tu instancia, base y secretos de desarrollo actuales. Esta revisión no cambia el esquema: no debes borrar la base ni crear otra cuenta Owner. No necesitas repetir el script de configuración para obtener otra clave.
4. Comprueba Negocio antes y después de crearlo; contrae y expande el menú varias veces y cambia de página. Repite con escalado 100 %, 150 % y 200 %, y moviendo la ventana entre pantallas.
5. Comprueba que una contraseña de confirmación distinta o un correo inválido mantienen abierto el editor.

## Alcance real y dependencias pendientes

Esta revisión corrige la interfaz existente; no declara terminado el primer sprint funcional completo.

El registro operativo actual de empleado todavía contiene identidad y contacto. No implementa asignación de sucursal, vínculo con cuenta, catálogo de permisos ni cambio de rol. Tampoco implementa subida de avatar o portada, salarios, áreas, inventario y caja. Los métodos de análisis con `NotImplementedException` siguen siendo bocetos y no rutas activas.

Para completar esos flujos se requiere una migración aditiva que preserve la base ya creada y relaciones comprobadas en el servidor:

- Sucursal → negocio del dueño autorizado.
- Empleado → sucursal autorizada; distinguir registro mínimo y asignación operativa.
- Cuenta opcional → empleado, con vínculo único. No listar al Owner ni cuentas ya vinculadas como candidatas.
- Permisos de administración o caja requieren cuenta válida. Empleados sin acceso pueden existir sin contraseña.
- Cambiar de puesto no borra cuenta ni hash. Si pierde permisos, denegar acceso en servidor y revocar sesiones afectadas, no limitarse a ocultar botones.
- Las cuotas de suscripción se validan en servidor al crear sucursales. Sigue vigente el bloqueo comercial y la habilitación explícita de prueba local de la entrega anterior.

## Pruebas

Compilación de API y cliente con SDK .NET 10.0.100: cero errores y cero advertencias. Se ejecuta también la prueba de transporte para 204, 200 vacío, JSON null, JSON válido y JSON inválido, y la comparación de contratos espejo.

Comandos reproducibles:

```powershell
dotnet build CoffeManager.slnx
dotnet run --project Tests/Transport/Transport.csproj
python Tests/check_contracts.py
# Con la API de desarrollo abierta:
python Tests/api_smoke.py
```

El entorno de revisión no dispone de escritorio Windows ni instancia SQL Server. Compilar WinForms en este entorno no equivale a probar su dibujo o escalado en Windows. Las operaciones reales de base de datos, creación de cuentas y persistencia siguen necesitando verificación con tu instancia SQL. Las pruebas de transporte usan respuestas simuladas y no afirman lo contrario.
