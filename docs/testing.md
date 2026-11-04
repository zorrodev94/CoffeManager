# Pruebas y estado de aceptación

## Estado actual

API y cliente compilaron; pasaron geometría, transporte, serialización/límites de perfiles, imágenes y estructuras espejo. Eso no constituye aceptación visual.

## Correcciones posteriores a las capturas

Se corrigió la inicialización de ProfileInfoCard: sus filas se crean antes de ejecutar layout, con protección durante construcción. También se protege la inicialización de UserProfileView. La suite de presentación construye ahora el perfil al principio para detectar esta regresión antes de recorrer activación.

Etiquetas y marca usan fondo transparente; el menú comparte una superficie uniforme. Autorización coloca sus acciones junto a los campos. Los selectores conservan el ComboBox como fuente de datos, pero usan una lista emergente local con desplazamiento oscuro, selección suave, ratón y teclado; no invocan el listado nativo blanco.

API, cliente y suite de presentación compilan sin errores ni advertencias. Geometría, transporte y espejos pasaron después de los cambios. **Pendiente:** ejecutar la suite WinForms y comprobar visualmente en Windows, especialmente el popup (flechas, F4/Alt+abajo, Escape, selección y cierre), autorización pequeña/maximizada y perfil con imágenes/datos completos. No se declara aprobado el recorrido SQL ni la ejecución Windows.

La captura de primera activación corresponde a vista previa; no acredita activación real contra SQL.

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

## Comprobación manual

Probar acceso y cierre de sesión, autorización y ambos registros; consulta, edición, guardado y lectura posterior del perfil; foto y portada; cancelación y errores; ventana pequeña/maximizada y monitores al 100%/200%.

## Presentación sin modificar cuentas reales

```powershell
dotnet run --project Tests/PresentationChecks/PresentationChecks.csproj
dotnet run --project Tests/PresentationChecks/PresentationChecks.csproj -- --preview-activation
```

El primer comando usa HTTP simulado; el segundo permite mirar activación sin crear un Owner. Las evidencias, responsables y resultados se registran en la incidencia correspondiente de GitHub. No marcar casos como aprobados sin ejecutarlos.

## Revisión de desplegables y fotografía

El fallo ObjectDisposedException reportado al seleccionar una opción se abordó liberando ToolStripDropDown después de que regrese su cierre nativo; los desplegables retirados también se liberan al destruir su dueño. PopupChecks repite selección/cierre/reapertura en empleado, sucursal y rol, incluyendo reapertura antes de liberar el anterior. Compila; ejecución de esta regresión pendiente en Windows. Los PASS de la suite anterior que mostró el usuario no cubrían ese recorrido del popup.

Se colocó el avatar delante de la portada y se corrigió el pintado de fondo de checkbox y contenedores de pares. La portada personalizada se muestra completa con márgenes si su proporción difiere; la fotografía predeterminada conserva el recorte de decoración. Las recomendaciones aparecen junto a las imágenes: avatar 512×512, portada 1600×400, JPG/PNG. El original admite hasta 20 MB y 20 MP; se normaliza a JPEG con lado máximo 1600 px y resultado máximo 2 MB. No se añadió reposicionamiento de recorte ni se cambió la estructura de la API.

## Revisión 5: comprobación en Windows

Esta entrega mantiene API, SQL, autenticación y datos existentes. No requiere migraciones ni reinicialización.

- Revisar icono en barra de tareas y ejecutable; Windows puede conservar en caché el icono de un acceso directo antiguo.
- Verificar perfil en ventana y maximizado a 100% y 200%: avatar sin cuadrado, encabezado compacto y portada que llena la franja mediante recorte centrado. La imagen original no se modifica al mostrarla.
- En registro, llegar al rol mediante Tab y abrirlo: todo el marco debe quedar por encima de las acciones fijas. Seleccionar, cerrar y volver a abrir varias veces.
- Revisar checkbox sin franja negra y bordes de tarjetas después de redimensionar.
- Guardar perfil, reiniciar cliente y confirmar datos e imágenes; probar registro real sin borrar el propietario existente.

La aceptación visual y la persistencia real en SQL siguen pendientes. Estos cambios no incorporan inventario ni amplían permisos.

## Revisión 6: cierre acotado

La descripción del encabezado ya no invade el botón Editar perfil. Se agregó una comprobación WinForms de contención y solapamiento a tres anchos. Compila aquí; debe ejecutarse en Windows junto con PresentationChecks.

En el editor, desplazar ya no cambia la altura de los campos a 2400 unidades y después a su altura real en cada paso. El fondo decorativo se dibuja una vez por tamaño/DPI y se libera al cerrar el panel. Confirmar fluidez a 200% y guardar, cancelar, mostrar errores y redimensionar para comprobar que el contenido se mantiene accesible.

Se fuerza pintura local plana del checkbox: revisar nuevamente la franja del registro. Las capturas de revisión 5 aún la mostraban; no se declara verificación visual de esta corrección.

Las capturas acreditan lectura y presentación de datos, no validación oficial de RFC/CURP. Los valores de prueba visibles no deben considerarse identificadores válidos; las reglas actuales de longitud no equivalen a validación de formato o existencia.

Pendientes para planear después de aceptar este cierre: negocio y sucursales; empleados, cuentas y asignaciones; inicio administrativo; cliente de tienda como prioridad alta empezando por acceso, sesión y sucursal, y después ventas e inventario con permisos por operación.

## Revisión 7: navegación repetida

En Windows ejecutar `dotnet run --project Tests/PresentationChecks/PresentationChecks.csproj`. Incluye selección repetida de Inicio, Mi perfil, Negocio, Sucursales y Empleados sin nuevas consultas sobre la opción activa; veinte plegados del menú con el editor abierto y conservación del texto sin guardar. Usa HTTP simulado, sin conexión a SQL.

Para aceptación visual: abrir Editar perfil, escribir sin guardar, plegar/desplegar repetidamente y pulsar Mi perfil. El editor debe mantenerse, sin aviso de descarte, fragmentos ni duplicación. Cambiar realmente a otra sección conserva la confirmación de descarte. Repetir en ventana y maximizado a 200%; comprobar que cada opción distinta abre su página. La opción activa no sirve como botón de actualización.

Compilación de solución y suite comprobada en Linux; ejecución WinForms y resultado visual pendientes de la última prueba del usuario. No se modifica el tablero ni se crean módulos nuevos en esta revisión.
