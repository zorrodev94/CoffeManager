# Auditoría · Acceso refinado

## Verificación ejecutada

- Solución .NET 10: compilación correcta, 0 errores y 0 advertencias.
- Estructuras espejo: las cuatro parejas coinciden. No se agregaron estructuras en Contracts.
- LayoutChecks: 18 combinaciones de viewport y DPI; regiones sin solapamiento ni salida del área. Tamaño inicial lógico 1160×760 convertido a 2320×1520 en 4K/200 %, y limitado al área de trabajo en laptop.
- Transport: respuestas correctas/vacías/malformadas y eliminación de cookies locales aunque logout reciba 503.
- DomainChecks: serialización de perfiles, límites, imágenes y rechazo de seis asignaciones incompletas antes de abrir SQL.
- Scripts SQL sin cambios respecto de Obsidiana Responsive. Tampoco cambian las estructuras de cuenta y login en esta revisión.

## Pendiente de ejecutar

No se ejecutó WinForms en este entorno Linux ni la integración SQL opcional. Las pruebas de geometría no son capturas reales ni verifican las medidas tipográficas renderizadas por Windows.

En tu equipo debe comprobarse: arranque restaurado y maximizado, 200 %/100 %, pestañas y teclado, error de conexión y reintento, autorización con dueño/administrador no dueño, cancelación, cuenta primero, empleado primero y cierre de la sesión temporal. No borres tu propietario actual para hacer estas pruebas.

Los largos formularios pueden requerir scroll vertical en ventanas pequeñas. La prueba del tamaño inicial no garantiza ausencia de scroll para todos los tamaños a los que el usuario reduzca manualmente la ventana.

## Cambios de seguridad del cliente

Crear cuenta de empleado, desde Acceso, autentica temporalmente al propietario y verifica IsOwner mediante la API. La autorización definitiva permanece en el endpoint protegido del servidor. La sesión temporal se cierra mediante finally al finalizar o cancelar el recorrido. ApiConnection expira las cookies locales también si no recibe confirmación del logout. Esto no revoca remotamente una cookie copiada fuera del cliente.

## Modelo

Se conservan los dos órdenes de creación. No se obliga a crear un empleado ficticio, no se enlaza por coincidencia de nombres y no se concede acceso por la mera existencia de una cuenta. Una vinculación inmediata se valida y guarda mediante la transacción de la entrega anterior.
