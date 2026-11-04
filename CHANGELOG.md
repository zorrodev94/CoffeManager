# Historial de cambios

## Sin publicar

- Revisión 7: pulsar la opción activa del menú no recrea la página ni vuelve a consultar la API; mantiene el editor abierto.
- Menú plegable con distribución agrupada, contenedor con doble búfer y repintado completo al cambiar el ancho.
- Los botones de navegación no vuelven a dimensionarse cuando su modo y DPI no cambiaron.
- Prueba WinForms de navegación repetida y conservación del editor, con transporte HTTP simulado.

- Revisión 6: acción Editar perfil separada del texto para evitar que una etiqueta la cubra.
- Editor: desplazamiento sin redimensionar todos los campos en cada paso; fondos decorativos reutilizados hasta cambiar tamaño o DPI.
- Checkbox con pintura local plana y regresión de solapamiento del botón a tres anchos.

- Revisión 5: icono de taza existente incorporado al ejecutable y a las ventanas.
- Perfil con encabezado compacto, avatar con región circular real y portada centrada que llena la franja sin deformarse.
- Selectores desplazan su marco completo al recibir foco o abrir opciones.
- Checkbox con superficie de fondo explícita y tarjetas que se repintan al cambiar de tamaño.

- Corregido el ciclo de cierre/liberación del desplegable; agregada regresión de selección y reapertura.
- Avatar colocado por delante de la portada; medidas recomendadas visibles (la revisión 5 cambia la portada a relleno proporcional).
- Ajustado fondo de checkbox y pares de campos.


- Documentación consolidada en README y docs/configuration.md, architecture.md y testing.md.
- Retiradas de la entrega las notas repetidas de ZIP/parches y los instaladores de parches anteriores.
- Conservados LICENSE, marca, banner y contenido principal del README nuevo.
- Corregida la construcción del perfil que causaba NullReferenceException; agregada su regresión de presentación.
- Eliminados fondos sólidos de etiquetas y marca, uniformado menú y acercadas acciones de autorización.
- Sustituido el listado emergente nativo por uno oscuro con teclado y desplazamiento local.
- Ejecución WinForms y persistencia real SQL pendientes de verificación en Windows.

## Base publicada

El historial de implementación se consulta en GitHub; no se reconstruyen fechas ni resultados a partir de notas de entregas antiguas.
