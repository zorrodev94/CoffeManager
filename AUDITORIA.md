# Auditoría de la revisión

## Alcance del cambio

La versión anterior mantenía el boceto con tipos anidados y un segundo modelo reducido para la API. Se integra el modelo activo para que las cuatro entidades usen estructuras completas anidadas, con espejo en el cliente. Se retiran estructuras de `Contracts` y coordinadores reducidos duplicados.

Registro y perfil se mantienen como etapas diferentes. Una consulta de perfil no devuelve contraseñas ni hashes. Un identificador proporcionado por cliente nunca establece por sí mismo el dueño o los permisos: el alcance se obtiene de la sesión.

## Matriz de campos activos

| Entidad | Creación mínima | Perfil completo conectado |
|---|---|---|
| Usuario | Nombre, apellidos, correo, contraseña y clave de activación del primer dueño | Nombre para mostrar, descripción, avatar, portada, teléfonos, nacimiento, domicilio, ciudad, estado, CP, RFC y CURP |
| Empleado | Nombre y apellidos | Correo, nombre para mostrar, descripción, avatar, portada, teléfonos, nacimiento, domicilio, ciudad, estado, CP, RFC, CURP, número de empleado, contratación, estado laboral, salario y periodicidad |
| Negocio | Nombre para mostrar y giro disponible | Nombre para mostrar, nombre legal, descripción, logo, portada, RFC, teléfono y correo |
| Sucursal | Negocio y nombre | Nombre, descripción, domicilio, colonia, ciudad, estado, CP, teléfono, correo, logo y portada |

Los campos de perfil tienen columnas SQL y parámetros de lectura/escritura explícitos. El verificador de base compara las estructuras con `INFORMATION_SCHEMA.COLUMNS`, sus tipos y longitudes. No se interpreta un `health 200` como prueba de persistencia.

## Pruebas ejecutadas aquí

- Compilación de API y WinForms con .NET SDK 10.0.100: cero errores y advertencias.
- Comparación de los bloques Structs de User, EmployeeUser, Business y Branch entre cliente y servidor; revisión de que no quedan estructuras en Contracts.
- Serialización y recuperación de todos los campos de los cuatro perfiles; rechazo de textos que exceden cada límite definido.
- Validación de formato/tamaño de imagen en el servidor.
- Pruebas de transporte para respuestas 204, 200 vacía, null, JSON válido, JSON inválido y respuesta de creación inválida.
- Pruebas de arranque, autorización anónima y validación HTTP sin utilizar una instancia SQL.

## Pruebas no ejecutadas aquí

El entorno no dispone de escritorio Windows ni una instancia SQL Server. Por ello no se afirma haber comprobado visualmente el título personalizado, el escalado, los diálogos de archivos o las operaciones reales de base de datos.

La prueba `Tests/DomainChecks` ejecuta validaciones de datos y, si se configura `COFFEMANAGER_TEST_SQL`, crea una base temporal aislada cuyo nombre genera el propio programa. Comprueba migración desde v1, inicialización repetida, conservación del hash del dueño, ida y vuelta de todos los campos de perfil, medios, cuentas disponibles, requisitos del rol, asignación única y retirada de acceso al cambiar de rol. Finalmente elimina exclusivamente esa base temporal y su carpeta temporal de imágenes. Esa rama SQL se omite cuando la variable no está configurada. Requiere permiso para CREATE DATABASE en la instancia; no usa tu base existente para las pruebas.

## Comandos reproducibles

Desde la carpeta de la solución:

```powershell
dotnet build CoffeManager.slnx
python Tests/check_mirrors.py
dotnet run --project Tests/Transport/Transport.csproj
dotnet run --project Tests/DomainChecks/DomainChecks.csproj
# Con la API de desarrollo abierta:
python Tests/api_smoke.py
# Sobre tu conexión configurada, sin modificar registros:
dotnet run --project CoffeManager.csproj --launch-profile https -- --verify-database
```

Para habilitar la rama de SQL aislada puedes definir en la sesión de PowerShell una cadena de conexión de tu instancia con autenticación de Windows. Por ejemplo, ajusta el nombre de instancia a tu equipo:

```powershell
$env:COFFEMANAGER_TEST_SQL = 'Server=localhost\SQLEXPRESS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True'
dotnet run --project Tests/DomainChecks/DomainChecks.csproj
Remove-Item Env:COFFEMANAGER_TEST_SQL
```

`TrustServerCertificate=True` se limita al ejemplo local de desarrollo. La prueba imprime PASS LIVE SQL solo si sus operaciones terminaron; SKIP no constituye una prueba SQL aprobada.

## Comprobación manual en Windows

1. Aplicar migración, verificar esquema y entrar con la cuenta existente.
2. Completar, guardar, cerrar y volver a consultar cada perfil; comprobar datos, avatar/logo y portada.
3. Probar valores opcionales vacíos y una longitud excesiva; comprobar que la validación conserva los datos del formulario.
4. Contraer/expandir menú, mover y redimensionar; minimizar, maximizar/restaurar y cerrar con teclado.
5. Repetir en escalados 100 %, 150 % y 200 %, cambiando entre monitores.
6. Registrar empleado y cuenta, asignar rol con acceso y sucursal; comprobar que no se listan Owner ni cuentas ya vinculadas. Cambiar a rol sin acceso, conservar la contraseña y comprobar el rechazo de la sesión anterior.

## Límites conocidos

El cliente de Caja no se incluye; los permisos de Caja se almacenan para su integración posterior. La licencia comercial no está implementada. El alta de sucursales continúa bloqueada fuera de la prueba explícita en Development. Las imágenes se validan por tamaño, formato y dimensiones de sus encabezados; no se promete un análisis completo del contenido de la imagen. El cliente vuelve a codificar las imágenes seleccionadas antes de subirlas.
