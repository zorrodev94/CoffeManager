> Entrega actual: lee primero `LEEME_HOME_OBSIDIANA.md`. Las siguientes notas describen revisiones anteriores y sus funciones conservadas.

# CoffeManager · Activación, acceso y cuentas

Esta revisión aplica el recorrido acordado en los tres bocetos, sobre la entrega Obsidiana Responsive. Conserva la base y el propietario existentes. No agrega invitaciones ni otro propietario.

## Ejecutar

Detén la API y el cliente anteriores. Extrae este ZIP en otra carpeta y abre PowerShell donde está CoffeManager.slnx:

```powershell
dotnet build CoffeManager.slnx
dotnet run --project CoffeManager.csproj --launch-profile https
```

En otra terminal:

```powershell
dotnet run --project CoffeManager.Client.Admin/CoffeManager.Client.Admin.csproj
```

No ejecutes Configure-Local.ps1 ni borres la base para mostrar este avance si ya tienes conexión, propietario y esquema 1/2 funcionando. Los scripts SQL no cambiaron en esta revisión. Ambos proyectos deben corresponder a esta entrega.

## Tres ventanas y sus estados

1. **Primera activación:** aparece únicamente si el servidor informa que Accounts está vacío. Nombre(s), apellidos, correo, contraseña, confirmación y clave. Propietario es fijo. Registrar no inicia sesión automáticamente.
2. **Acceso activado:** correo y contraseña, Iniciar sesión y Crear cuenta de empleado. Con tu propietario existente verás directamente este estado. El estado de conexión está en un pie discreto; Reintentar solo aparece si falla.
3. **Cuenta de empleado:** Crear cuenta de empleado abre primero una autorización con credenciales del propietario. Al confirmarla, permite registrar la cuenta. Esa sesión temporal se cierra al salir del recorrido. El endpoint del servidor mantiene su exigencia de propietario.

La autorización del propietario no es una segunda activación y no pide la clave inicial. No altera la contraseña ni el perfil del dueño. El registro también continúa disponible dentro de Administración → Empleados para el propietario.

## Ambos órdenes siguen siendo válidos

- **Empleado primero:** puede existir sin cuenta ni contraseña. Desde el registro exterior del acceso, el dueño puede elegir Vincular ahora a un empleado existente y seleccionar empleado, sucursal y rol.
- **Cuenta primero:** desmarca Vincular ahora. Se guardan las credenciales sin crear silenciosamente un empleado. Después, desde Empleados, se agrega el perfil laboral y se vincula la cuenta disponible mediante la acción de asignación existente.

El rol operativo se aplica cuando se vincula la cuenta al empleado y a su sucursal. Por eso el selector se muestra en la sección de vinculación; una cuenta sin vincular todavía no obtiene acceso operativo. Este comportamiento preserva las relaciones existentes: no hay un rol independiente duplicado en Accounts.

Los roles se leen de SQL con sus nombres españoles: Administrador, Encargado, Cajero y Empleado, más los que el propietario agregue. Agregar rol conserva el editor autorizado y no crea propietarios. Los códigos internos se mantienen estables.

## Diseño aplicado

- Tamaño inicial definido en unidades lógicas y convertido una vez al DPI del monitor antes de ajustar al área de trabajo.
- Login sin marca repetida dentro del formulario; título y mensaje más breves.
- Arte local nuevo de cafetería realista al atardecer. No contiene textos ni controles incrustados.
- Campos limitados en anchura y controles más compactos en ventanas bajas. La fotografía pasa a una franja lateral estrecha antes de quitar altura al formulario.
- Sin cabecera fotográfica que empuje el login hacia abajo.
- Pie de conexión separado del contenido y botón de ojo dibujado localmente para contraseña.
- Formularios largos conservan desplazamiento vertical en ventanas pequeñas; no se comprimen los datos para ocultar campos obligatorios.

La implementación es WinForms con controles reales, no una imagen completa clicable ni WebView. Los bocetos siguen siendo referencia visual, no una captura de ejecución.

## Qué probar en tu equipo

1. Ventana recién abierta, restaurada y maximizada al 200 % en tu pantalla 4K.
2. Laptop al 100 % y, si es posible, cambio entre monitores.
3. Con tu propietario ya creado: activación oculta; login y Crear cuenta de empleado presentes.
4. Crear cuenta de empleado → cancelar autorización; credenciales incorrectas; propietario correcto.
5. Cuenta sin vincular; vinculación a empleado existente; correo duplicado; rol nuevo.
6. Detener API y pulsar Reintentar cuando aparezca el error.

No se ejecuta WinForms ni SQL Server en este entorno. La compilación y los cálculos de distribución no sustituyen comprobar la apariencia real ni las inserciones en tu equipo. Consulta AUDITORIA_ACCESO_REFINADO.md.
