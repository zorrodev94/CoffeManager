> Entrega más reciente: **LEEME_ACCESO_REFINADO.md**. El texto siguiente conserva el alcance de su revisión anterior.

# CoffeManager · Obsidiana, distribución y cuentas

Revisión del 3 de noviembre de 2026, posterior a LEEME_OBSIDIANA.md.

## Ejecutar conservando tu información

Detén API y cliente, conserva la carpeta anterior y extrae esta entrega en otra carpeta. No borres tu base, cuentas ni secretos. Los scripts SQL no cambiaron.

Abre PowerShell donde está CoffeManager.slnx:

```powershell
dotnet build CoffeManager.slnx
dotnet run --project CoffeManager.csproj --launch-profile https
```

En otra terminal:

```powershell
dotnet run --project CoffeManager.Client.Admin/CoffeManager.Client.Admin.csproj
```

Usa ambos proyectos de esta entrega. Cambió el cuerpo opcional del registro de cuentas para incluir EmployeeId, BranchId y RoleId. Si tu base ya tiene las versiones 1 y 2, no requiere otra migración. La configuración y tus cuentas existentes siguen siendo válidas.

## Qué corregimos a partir de las capturas

- El menú aplica su ancho y dimensiones al abrir, no solamente cuando se pulsa la hamburguesa.
- El acceso utiliza medidas explícitas según DPI y una columna de campos con ancho máximo de 520 unidades lógicas.
- Se reemplazó el FlowLayoutPanel del acceso por una distribución vertical controlada. El contenido no necesita desplazamiento horizontal.
- El login se centra verticalmente cuando hay espacio. Formularios largos permiten desplazamiento vertical.
- En ventanas anchas, la fotografía permanece al lado. En ventanas estrechas, se convierte en una cabecera: ya no desaparece por completo.
- Los pares de apellido y contraseña pasan a filas individuales cuando no hay ancho suficiente.
- Campos y acciones se dimensionan a 52 unidades lógicas. El texto usa puntos tipográficos y se conserva navegación de teclado.
- La paleta oscura se aplica también a Administración y los editores existentes. No incorpora el mapa 3D ni una réplica completa del dashboard conceptual.
- MarfilCafe conserva su nombre interno para no romper todas las referencias anteriores; sus valores ahora delegan en la paleta Obsidiana. Es compatibilidad de código, no el nombre mostrado al usuario.

## Registrar la cuenta de un empleado

Inicia sesión como propietario. Abre Empleados → Registrar cuenta de acceso.

El nuevo formulario pide nombre(s), apellidos, correo, contraseña y confirmación. Puede seleccionar un empleado existente que todavía no tenga cuenta; al elegirlo, toma su nombre y apellidos para facilitar el llenado.

Hay dos rutas explícitas:

1. **Cuenta preparada para vincular después:** deja empleado, sucursal y rol sin asignar. Se crea la cuenta sin acceso operativo, como en la entrega anterior.
2. **Cuenta vinculada ahora:** elige los tres: empleado, sucursal y rol. El servidor valida que el empleado y la sucursal correspondan al propietario, que el empleado no tenga otra cuenta y que el rol exista. Inserción y vinculación se guardan dentro de una transacción: si falla alguna parte, no queda la cuenta creada a medias.

No se crea silenciosamente un segundo empleado con el mismo nombre: primero agrega el empleado mínimo desde el botón existente si todavía no está registrado.

El acceso efectivo continúa dependiendo del rol y del estado laboral. Cajero tiene acceso de Caja, no de Administración; el cliente de Caja sigue pendiente. Un rol sin acceso o un empleado suspendido no obtiene acceso por tener contraseña.

Los nombres españoles ya vienen del catálogo SQL: Administrador, Encargado, Empleado y Cajero. Los códigos internos ingleses permanecen estables. “Agregar otro rol” abre el editor autorizado para que el propietario escriba nombre o apodo y configure sus accesos. Un nombre que diga Dueño no convierte la cuenta en propietaria: las cuentas posteriores se insertan con IsOwner=0.

## Código cambiado para estudiar

- Presentation/Controls/AccessStackPanel.cs: tamaño y posición de campos, etiquetas, pares y desplazamiento vertical.
- Presentation/Theme/AccessLayoutMetrics.cs: elección de columnas o cabecera según espacio y DPI.
- Presentation/Windows/ObsidianAccessForm.cs: aplica esa distribución y conserva la fotografía.
- Presentation/Windows/StudioForm.cs: medidas de barra de título después del arranque/cambio de monitor.
- Presentation/Forms/AdminForm.cs: aplica el menú desde el primer arranque y abre StaffAccountForm.
- Presentation/Forms/StaffAccountForm.cs: formulario completo de cuenta para empleado.
- Models/User/Base/User.cs, en ambos proyectos: tres identificadores opcionales en StaffAccountRegistrationData, manteniendo las estructuras espejo.
- Models/User/Repositories/AccountAccessRepository.cs, servidor: transacción de cuenta y vinculación opcional.

## Qué se comprobó

Compilación de ambos proyectos, coincidencia de estructuras espejo y pruebas existentes de transporte/validación. Las pruebas de geometría verifican paneles sin solapamiento ni salida del viewport en 18 combinaciones de tamaño/DPI.

**Límite de verificación:** este entorno no ejecuta WinForms ni SQL Server. Esas pruebas no sustituyen revisar la apariencia en tu Windows, ni confirman la nueva transacción contra tu instancia real. Se añadió una regresión SQL opcional para creación vinculada y rechazo de empleado ya vinculado; requiere COFFEMANAGER_TEST_SQL y una instancia de pruebas.

## Revisión en tu equipo

Prueba ventana restaurada y maximizada, 4K al 200 %, laptop al 100 % y, si puedes, cambio entre monitores. Confirma que no aparezca desplazamiento horizontal, que la foto siga visible y que puedas alcanzar todos los campos con desplazamiento vertical.

Después comprueba: cuenta sin vincular, cuenta con empleado/sucursal/rol, correo duplicado y empleado ya vinculado. No reutilices el registro del propietario para crear otra cuenta.
