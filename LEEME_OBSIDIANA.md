> Entrega más reciente: **LEEME_ACCESO_REFINADO.md**. El texto siguiente conserva el alcance de su revisión anterior.

> Revisión más reciente: lee primero **LEEME_RESPONSIVE.md**. Este documento conserva el alcance de la entrega anterior.

# CoffeManager · Acceso Obsidiana

Entrega del 3 de noviembre de 2026. Este avance implementa el tema oscuro en Acceso y Primera activación. El resto de Administración conserva su presentación anterior. No incluye invitaciones ni registro público de empleados.

## Cómo probar con tu base actual

1. Detén API y cliente. Conserva tu carpeta anterior y extrae esta entrega en otra carpeta.
2. Abre PowerShell en la carpeta que contiene `CoffeManager.slnx`.
3. Ejecuta:

```powershell
dotnet build CoffeManager.slnx
dotnet run --project CoffeManager.csproj --launch-profile https
```

4. Deja la API abierta y ejecuta, en otra terminal:

```powershell
dotnet run --project CoffeManager.Client.Admin/CoffeManager.Client.Admin.csproj
```

También puedes iniciar el proyecto cliente desde Visual Studio. API y cliente deben pertenecer a esta entrega para disponer de la nueva consulta de activación.

No cambió el esquema SQL: si tu base ya verificó las versiones 1 y 2, no necesitas reinicializarla ni ejecutar Configure-Local.ps1 otra vez. Se conservó el identificador de secretos del proyecto. Si estás en otro equipo o perfil de Windows, consulta INICIO_CLIENTE.md para configurar ese entorno.

Si ya existe tu cuenta, la consulta al servidor oculta Activar primera cuenta. Inicia sesión con tus credenciales actuales. No borres cuentas para mostrar una activación; usa una base de pruebas separada si necesitas demostrar la creación inicial.

## Qué cambió

- Paleta carbón, texto claro, acento menta y fotografía arquitectónica nocturna generada para este proyecto.
- Arte incluido como recurso local del cliente. No descarga fondos al abrir ni renderiza una escena 3D.
- Campos completos: nombre(s), apellido paterno, materno opcional, correo, contraseña, confirmación y clave.
- Apellidos y contraseñas en pares; en ventanas cortas hay desplazamiento. En ventanas estrechas se oculta el panel decorativo para dar prioridad al formulario.
- Botones para mostrar u ocultar contraseñas, foco visible y navegación con teclado.
- Errores de validación dentro del formulario; los datos permanecen cuando falla el envío.
- Propietario se muestra como información fija: lo asigna SQL con las reglas existentes, no una elección enviada por el cliente.
- Estado de conexión basado en una consulta real al servidor y a Accounts. Es el resultado de la última comprobación, no una garantía permanente de conectividad.
- Botón Comprobar conexión para reintentar. No hay temporizadores ni animaciones.
- Consulta GET api/accounts/setup-required sin identificadores, correos ni secretos. Tiene límite independiente de las peticiones de login.
- El bloqueo definitivo de otra activación sigue en el repositorio y SQL; ocultar el botón no lo reemplaza.

## Código para estudiar

| Archivo | Trabajo |
|---|---|
| Presentation/Forms/AccessForm.cs | Coordinación de login, estado y activación |
| Presentation/Forms/AccountActivationForm.cs | Campos, confirmación, validación y envío antes de cerrar |
| Presentation/Windows/ObsidianAccessForm.cs | Distribución y desplazamiento del acceso |
| Presentation/Theme/Obsidian.cs | Paleta y fábricas de controles |
| Presentation/Controls/ObsidianField.cs | TextBox nativo con superficie y visibilidad de contraseña |
| Presentation/Controls/ObsidianButton.cs | Dibujo y estados de los botones |
| Models/User/Base/User.cs (cliente) | IsSetupRequiredAsync y las operaciones de cuenta existentes |
| Infrastructure/Api/ApiEndpoints.cs (servidor) | Endpoint de consulta del estado inicial |
| Models/User/Repositories/AccountRepository.cs | Consulta de existencia de cuentas |

Las primeras seis rutas son del proyecto cliente. Los tipos de estructuras de interacción no cambiaron. Las dependencias de transporte y persistencia conservan sus funciones anteriores.

Recorrido de activación: AccessForm.RegisterAsync abre AccountActivationForm y entrega una función que llama a RegisterInitialAsync. Al pulsar Activar cuenta, ConfirmAsync valida, espera el resultado del servidor y solo cierra con OK al terminar correctamente. Después AccessForm vuelve a consultar el estado y prepara el correo para iniciar sesión. Registrar no inicia sesión automáticamente.

## Qué mostrar al maestro

“Este avance integra la presentación del acceso con el registro inicial persistente. El cliente valida y envía los datos por HTTPS; el servidor valida la clave, genera el hash y registra al propietario mediante una transacción SQL. La ventana consulta si la activación está disponible y la oculta cuando ya existe una cuenta. Estamos desarrollando y revisando el sistema por interacciones, comenzando por activación, login y sesión.”

Si ya tienes cuenta, muestra el login funcionando y el botón de activación oculto. Puedes mostrar el código y el boceto del formulario inicial sin afirmar que realizaste otra activación en esa misma base.

## Validación y límites

- Compilación de ambos proyectos en .NET 10: correcta, sin errores ni advertencias.
- Comprobación de estructuras espejo: correcta.
- Pruebas de transporte y validación: consultar AUDITORIA_OBSIDIANA.md.
- En este entorno no se ejecuta WinForms ni SQL Server. La apariencia exacta en Windows, cambio de monitor y escalados 100/150/200 % requiere revisión en tu equipo.
- Conserva desplazamiento vertical para que todos los campos puedan alcanzarse en pantallas pequeñas o con escalado elevado.
- No se agregó tema día/noche, mapa operativo 3D ni nuevos permisos.
- No se promete que esta entrega complete el sistema comercial: mantiene los pendientes de la versión anterior.
