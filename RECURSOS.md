# Recursos visuales de esta iteración

Tema de trabajo: Marfil y café, pendiente de validación final del equipo.

## Archivos incluidos
- `CoffeManager.Client.Admin/Resources/Brand/fox-coffee.png`: zorro con taza sobre transparencia, 1254 × 1254 píxeles. Identidad predeterminada de CoffeManager, no foto personal del cliente.
- `CoffeManager.Client.Admin/Resources/Brand/cafe-access.png`: fotografía ilustrativa generada para acceso e inicio, 1672 × 941 píxeles. No se presenta como una fotografía de un negocio real ni como una captura del programa.

Ambos se generaron mediante la herramienta integrada de imágenes de OpenAI y se incorporaron como recursos embebidos de la aplicación. No contienen marcas de cafeterías de terceros. El ZIP incluye los archivos fuente de imagen originales, sin rutas dependientes de mi equipo.

## Prompts de producción
1. Original polished coffee shop management software mascot logo: a friendly clever orange fox curled around a small ivory ceramic espresso cup, espresso brown outlines, warm amber and ivory highlights. Isolated on genuinely transparent background, no text, no seasonal elements. Brand CoffeManager by ZDesarrolloVirtual.
2. Premium editorial image of a welcoming modern Mexican café and pastry shop, warm ivory walls, espresso wood, soft golden daylight, latte and croissant in the right foreground, low-detail cream space on the left. No text, logos, people or seasonal graphics.

## Escalado
Los formularios declaran PerMonitorV2, referencia DPI 96 y distribución por contenedores con desplazamiento. Al mostrarse y cambiar de DPI ajustan su tamaño al área útil del monitor, para que el mínimo de ventana no obligue a salir de una laptop con escalado alto. Los textos se dibujan como controles, no están pegados en fotografías. Los logos se muestran con Zoom; el fondo de acceso usa un recorte proporcional Cover para llenar su panel sin deformarse.

La fotografía no tiene resolución nativa 4K. No se amplió artificialmente para afirmar que la tiene. Se debe evaluar su aspecto en la pantalla de 55 pulgadas; el tamaño físico no determina por sí solo el área de trabajo. Falta probar 100 %, 150 % y 200 % en Windows real. No se garantiza aún fidelidad exacta a los mockups ni todos los layouts en cualquier escalado.

Las fotos cargadas por usuarios, límites de peso y recorte de avatar/portada pertenecen a una futura integración Media. Estos recursos son predeterminados del programa, no archivos que el cliente pueda sobrescribir desde la API.


## Controles de esta revisión

Iconos de navegación, avatar neutro, botones de título, superficies y botones de acción se dibujan localmente con GDI+ y controles WinForms. No requieren imágenes externas ni descargas. Se conservan `fox-coffee.png` y `cafe-access.png`.

Las fotos elegidas por usuarios se normalizan en el cliente y se guardan en el servidor bajo `App_Data/Media` por defecto, fuera del acceso público estático. Puede configurarse `Media:RootPath` en el servidor. Respalda esa carpeta junto con la base SQL. El proyecto excluye App_Data de publicación: al actualizar no reemplaces tu carpeta persistente con una copia vacía.
