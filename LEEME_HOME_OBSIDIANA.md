# CoffeManager — Home Obsidiana, revisión 04

Esta entrega modifica el cliente del ZIP recibido. Conserva el fondo oscuro y el acento menta con una distribución de tarjetas inspirada en la propuesta 2 elegida. No incorpora el color azul de aquella propuesta.

## Qué cambió

- Home sin fotografía decorativa: negocio, número real de sucursales y empleados en tarjetas.
- Acciones para agregar empleado, gestionar sucursales y registrar cuenta. Registrar cuenta se muestra al propietario; no crea otro dueño.
- Menú sin fotografía usada como logo: símbolo local de taza y navegación vectorial dibujada en el cliente, sin emoji ni descargas.
- Página activa resaltada en menta. Hamburguesa sin texto, modo compacto con iconos y ayuda al pasar el cursor. Cierre de sesión al pie cuando hay espacio.
- Tarjetas en tres, dos o una columna según el ancho disponible y el DPI. Inicio dimensionado en unidades lógicas, ajustado al área de trabajo del monitor. No cambies tu escalado de Windows para probarlo.
- Autorización del dueño en un diálogo más pequeño, sin fotografía, antes de registrar una cuenta desde el acceso.
- Mayor ancho de formulario para primera activación y registro. Se conservan todos sus campos y las parejas de apellidos/contraseñas. En ventanas estrechas se apilan y permiten desplazamiento vertical.

El resto de las vistas conserva su funcionamiento anterior. Esta revisión no declara rediseñados el perfil completo, negocio, sucursales ni los editores genéricos. El dibujo conceptual y la aplicación aún requieren comparación visual en Windows.

## Tu base de pruebas ya fue borrada

Si ejecutaste también CREATE DATABASE CoffeManager, no vuelvas a borrarla. Si solo la borraste, en SSMS ejecuta Infrastructure/Database/Scripts/000_create_database.sql para crear la base vacía.

Cierra cliente y API. Extrae esta entrega en una carpeta nueva. Abre PowerShell donde está CoffeManager.slnx y ejecuta, uno por uno:

```powershell
dotnet build CoffeManager.slnx
dotnet run --project CoffeManager.csproj --launch-profile https -- --initialize-database
dotnet run --project CoffeManager.csproj --launch-profile https -- --verify-database
```

Si conservas tu conexión y clave de activación en secretos de desarrollo, siguen funcionando al usar el mismo usuario de Windows. Si no guardaste la clave, antes de activar ejecuta:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Configure-Local.ps1 -SqlServer 'localhost\SQLEXPRESS'
```

Ese comando configura conexión local, genera otra clave de desarrollo y solicita confiar en el certificado. No crea tablas ni borra datos. Guarda la clave mostrada y no la publiques.

Levanta la API:

```powershell
dotnet run --project CoffeManager.csproj --launch-profile https
```

En otra terminal, desde la misma carpeta:

```powershell
dotnet run --project CoffeManager.Client.Admin/CoffeManager.Client.Admin.csproj
```

## Recorrido para mostrar al maestro

1. Base inicializada y vacía: el acceso ofrece Activar primera cuenta tras comprobar el servidor.
2. Captura nombres, apellidos, correo, contraseña, confirmación y clave. El propietario se asigna en el servidor; no se elige libremente.
3. Inicia sesión: Home muestra los datos existentes. En una base nueva, los contadores deben ser cero y el negocio aparece por configurar.
4. Contrae y expande el menú, cambia de ventana, maximiza/restaura y mueve entre monitores. Prueba tu 200 % y laptop 100 %.
5. Desde Home, el dueño puede registrar una cuenta con nombres, apellidos, correo y contraseña/confirmación. Si existe un empleado sin cuenta, puede vincularlo con sucursal y rol en español.
6. Si aún no hay empleado, puede crear primero la cuenta y vincularla después. La cuenta sola no concede acceso operativo. El perfil laboral y el registro de credenciales son operaciones distintas.
7. Cierra sesión. La primera activación ya no debe ofrecerse al consultar nuevamente el servidor.

No hace falta volver a borrar la base para revisar tamaños o colores. No se cambió el esquema ni las reglas de acceso en esta revisión.

## Actividad del sistema

El panel es un espacio preparado, con un aviso explícito de que el historial llegará en otra entrega. No se registran eventos nuevos ni se muestran ventas, registros o borrados ficticios. Los eventos auditables necesitan persistencia y consulta en el servidor antes de presentarlos como historial.

## Verificación de esta entrega

- Cliente net10.0-windows: compilación correcta, cero errores y advertencias.
- Cuatro pares de estructuras espejo: correctos. Contracts sin estructuras de interacción.
- Cálculos del acceso: 18 combinaciones de tamaño/DPI, regiones dentro del contenedor y sin superposición; inicio a 200 % y límites de laptop.
- Cálculos del Home: 64 combinaciones de ancho/DPI/número de acciones, tarjetas dentro del ancho y sin superposición.
- Transporte: respuestas vacías/JSON válido e inválido y limpieza de cookies aun cuando falla logout.
- No se ejecutó WinForms ni SQL Server en este entorno. Las pruebas geométricas no prueban renderizado, foco, interacción ni apariencia real.
- No se volvió a validar la compilación del servidor: la restauración de sus paquetes no estuvo disponible aquí. Sus fuentes, dependencias y scripts SQL se conservaron sin cambios frente al ZIP recibido.

Archivos principales: AdminForm.cs, HomeDashboard.cs, NavigationButton.cs, DashboardLayoutMetrics.cs, AccessStackPanel.cs, ObsidianAccessForm.cs y OwnerAuthorizationForm.cs.
