# Obsidiana — recursos iniciales

- `Illustrations/home-cup-mint.png`: taza transparente para Inicio. Conservar proporción y limitar su tamaño. Ocultar la decoración cuando haga falta espacio para los datos. No es un botón ni el logotipo del menú.
- `Backgrounds/obsidian-contours-background.svg`: fuente vectorial del fondo grafito con curvas suaves, 2560 × 1600. WinForms no carga SVG de forma nativa; integrar mediante dibujo GDI+ equivalente o exportación PNG. No añadir una biblioteca únicamente para este fondo.
- `theme-assets.json`: inventario con nombres y procedencia.

Son recursos locales del cliente: no van a SQL ni se solicitan al servidor. Portada y avatar elegidos por el usuario conservan su flujo de medios. Se conserva la fotografía actual del acceso. Los iconos y botones mantienen dibujo vectorial para adaptarse al DPI.

Integrado: `HomeWelcomePanel` carga la taza como EmbeddedResource mediante `BrandResources.HomeCup`. `ObsidianBackdrop` dibuja el fondo equivalente con GDI+ según el tamaño actual; el SVG queda como fuente editable, sin dependencia adicional. No duplicar archivos en Brand.

## Evidencias XP

QA-01 verifica activación, login/logout, registro autorizado y acceso al panel. Puede ejecutarse mientras se corrige la interfaz. Registrar responsable, fecha, commit o cambios locales probados, pasos, esperado, obtenido, aprobado/fallido y captura. Las fotos del equipo documentan la sesión; las capturas muestran el resultado de cada prueba.

El perfil debe tener una historia y casos de consulta, edición, imágenes, validaciones, cancelación y lectura posterior. No marcar aprobado un caso sin ejecutarlo. No se modificó el tablero de GitHub.
