# Arquitectura y mapa de estudio

| Responsabilidad | Servidor | Cliente administrativo |
|---|---|---|
| Usuario: estructuras, cuenta, perfil y roles | Models/User/Base/User.cs | Models/User/Base/User.cs |
| Empleado: estructuras y operaciones específicas | Models/User/Classes/EmployeeUser.cs | Models/User/Classes/EmployeeUser.cs |
| Negocio | Models/Business/Classes/Business.cs | Models/Business/Classes/Business.cs |
| Sucursal | Models/Business/Classes/Branch.cs | Models/Business/Classes/Branch.cs |

Las rutas del cliente son relativas a CoffeManager.Client.Admin. Los bloques Structs son iguales, incluidas las anotaciones. Las operaciones están separadas por regiones. En Operation Data se mantienen las etiquetas de las operaciones; esas etiquetas no conceden permisos.

La clase User es abstracta en ambos proyectos. Account en servidor y User en Classes del cliente son puntos de construcción concretos que heredan la base completa. No sustituyen ni duplican sus estructuras. Employee es la clase concreta que hereda EmployeeUser.

El servidor recibe los identificadores de dueño/usuario desde la sesión. Esos parámetros no se envían libremente desde los formularios. El cliente envía datos de interacción; el servidor aplica reglas y repositorios. Sus cuerpos de método son diferentes deliberadamente.

Los arreglos de inicialización solo existen en servidor, porque el cliente no crea tablas ni siembra catálogos. Contracts se reserva para interfaces; Services sigue reservado para cuando exista una responsabilidad que lo necesite.

Los perfiles de negocio, sucursal y empleado usan RecordEditor<T>. Mi perfil usa UserProfileView y UserProfileEditor con UserProfileData. El perfil de usuario no solicita contraseña ni cambia la identidad de login; la asignación laboral y el acceso a cuentas están en una operación separada del perfil de empleado.

## Recorridos

Cliente WinForms → ApiConnection HTTPS → API autenticada → repositorios → SQL Server. El servidor determina el usuario y alcance desde la sesión.

- Activación: consulta setup-required y, sólo cuando corresponde, bootstrap crea al primer Owner.
- Acceso: login y cookie temporal; logout limpia la cookie local incluso si falla la petición.
- Registro: acceso externo exige autorización del Owner; el interno reutiliza la sesión. Ambos usan StaffAccountForm.OpenAsync y catálogos de la API.
- Perfil: User.GetAsync → UserProfileView → UserProfileEditor → User.UpdateAsync → nueva consulta. Avatar y portada se almacenan mediante el servicio de medios.
- Interfaz: StudioForm aloja páginas; AccessStackPanel mide campos; ObsidianField/Selector/CheckBox comparten controles; ObsidianScrollPanel controla desplazamiento; HomeDashboard presenta datos reales.

Las suscripciones comerciales permanecen separadas del perfil personal.
