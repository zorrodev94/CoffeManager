# CoffeManager — entidades espejo y formularios completos

Esta revisión integra el modelo completo con las operaciones activas. Las estructuras de interacción están dentro de sus entidades y se repiten deliberadamente en cliente y servidor. `Contracts` queda reservado para interfaces. No se añade un proyecto de contratos compartidos.

## Primero: actualizar el esquema existente

Detén API y cliente. Extrae en una carpeta nueva y conserva la copia anterior. Abre PowerShell donde está `CoffeManager.slnx`. Conserva tus secretos, cuenta Owner y base actuales.

```powershell
dotnet build CoffeManager.slnx
dotnet run --project CoffeManager.csproj --launch-profile https -- --initialize-database
dotnet run --project CoffeManager.csproj --launch-profile https -- --verify-database
```

La inicialización aplica `001_initial.sql` y `002_complete_profiles.sql`, guarda sus versiones y prepara los catálogos una sola vez. La versión 2 agrega columnas y relaciones; no borra cuentas, hashes, empleados, negocios ni sucursales. Es una actualización necesaria para esta entrega. Si una verificación falla, conserva el mensaje completo antes de seguir.

Después inicia la API y el cliente:

```powershell
dotnet run --project CoffeManager.csproj --launch-profile https
# En otra terminal:
dotnet run --project CoffeManager.Client.Admin/CoffeManager.Client.Admin.csproj
```

No vuelvas a activar el primer dueño si ya existe. Esta revisión cambia las validaciones de sesión: inicia sesión nuevamente después de actualizar ambos proyectos.

## Organización corregida

En ambos proyectos:

- `Models/User/Base/User.cs`: estructuras de registro, login, perfil, consulta, cuentas disponibles y roles; operaciones de cuenta, perfil y rol.
- `Models/User/Classes/EmployeeUser.cs`: estructuras de creación mínima, perfil completo, asignación y consulta de empleado; hereda de User.
- `Models/User/Classes/Employee.cs`: implementación concreta de empleado.
- `Models/Business/Classes/Business.cs`: estructuras y operaciones de creación y perfil del negocio.
- `Models/Business/Classes/Branch.cs`: estructuras y operaciones de creación y perfil de sucursal.

El servidor recibe, valida y persiste mediante los repositorios de cada entidad. El cliente construye la estructura equivalente y solicita la operación por HTTPS. Sus métodos no incluyen SQL ni hashes. Las dependencias de transporte del cliente y persistencia del servidor difieren de forma intencional.

`Models/EntityStructures.cs` contiene únicamente importaciones de tipos anidados; no declara estructuras ni recoge datos de distintas entidades. `Documentation/PreviousAnalysis` conserva el boceto anterior como texto de referencia, fuera de las clases ejecutables.

## Formularios y presentación

- Tema café/marfil conservado, sin mostrar su nombre en la aplicación.
- Barra de título propia con botones vectoriales, mover, redimensionar, minimizar y maximizar dentro del área de trabajo del monitor. Redondeo nativo disponible en Windows 11.
- Campos de texto sin borde clásico, dentro de superficies con borde suave y foco visible; botones redondeados con estados de interacción y teclado.
- Menú compacto de iconos sin temporizadores ni animaciones.
- `RecordEditor<T>` construye un formulario a partir de una estructura de interacción concreta: no mezcla las estructuras de distintas ventanas. Cada propiedad tiene editor y etiqueta en español; aplica límites y validación antes de cerrar.
- Perfiles completos de usuario, negocio, empleado y sucursal. Las creaciones iniciales siguen siendo mínimas.
- Campos de fecha opcionales con casilla, salario decimal y selección de opciones persistidas. Las contraseñas nuevas solicitan confirmación.
- Avatar, logo y portada seleccionables. El cliente prepara JPEG de hasta 1600 píxeles y 2 MB; SQL conserva identificador y propietario y el servidor conserva el archivo privado.

## Flujo de empleados y cuentas

1. Agrega el empleado con nombre y apellidos.
2. Completa su información personal/laboral cuando la tengas.
3. Si va a usar Administración o Caja, el dueño registra primero una cuenta de acceso. Esa cuenta no recibe acceso por existir.
4. Asigna sucursal, rol y cuenta al empleado. El servidor exige cuenta y sucursal para roles con acceso y evita reutilizar una cuenta ya asignada. El Owner no aparece como candidato.
5. Al cambiar el rol se incrementa la versión de autenticación de la cuenta afectada. Se conserva el hash. Un rol sin permiso ya no entra a Administración; las sesiones anteriores se rechazan en su siguiente petición.

Los catálogos de roles, periodicidades y estados laborales viven en SQL y permiten agregar opciones. Los valores iniciales provienen de los arreglos marcados `Obsolete`, usados por la inicialización controlada; las operaciones normales consultan SQL. Solo el dueño crea cuentas/roles/catálogos y asigna acceso. Un administrador con permiso puede gestionar los perfiles operativos.

`Activo` y el estado no especificado permiten acceso según el rol. Los estados Pendiente, Permiso temporal, Suspendido, Finalizado y los estados nuevos no reconocidos como Active no conceden acceso. Cambiar el puesto o el estado laboral no borra la contraseña.

## Lo que sigue fuera de esta entrega

Áreas, tareas y sus historiales, inventarios, recetas, bienes, movimientos de dinero y el cliente de Caja. La estructura UserSubscriptionData y la propiedad Subscription se conservan como diseño reservado del proveedor; no se integran en los formularios de creación o perfil. Suscripciones comerciales y cuotas efectivas siguen pendientes. La creación de sucursales mantiene la habilitación explícita de desarrollo de la entrega anterior; consulta `INICIO_CLIENTE.md`.

No se declara finalizado el sistema comercial completo. Las imágenes que se suben y luego se descartan antes de guardar pueden quedar sin referencia: la recolección de esos archivos y los límites globales de almacenamiento requieren una siguiente operación de mantenimiento. No hay eliminación automática de imágenes utilizadas.

## Verificación

Consulta `AUDITORIA.md` para distinguir las pruebas ejecutadas de las pendientes en Windows/SQL Server y `TODO.md` para el siguiente alcance.
