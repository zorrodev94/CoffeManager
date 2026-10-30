# Inicio y actualización local

Lee primero `LEEME_CAMBIOS.md`. Esta revisión requiere aplicar la migración 002 antes de usar el cliente. Si ya tienes conexión y Owner, conserva ambos; continúa directamente con la inicialización de esquema y la verificación. No necesitas regenerar la clave de activación.

# CoffeManager: primer recorrido conectado en localhost

## Requisitos
Windows, Visual Studio compatible con .NET 10, carga de trabajo de escritorio .NET y SQL Server/Express instalado. SQL y la API se ejecutan en el mismo equipo para este recorrido. El cliente solo conoce la API, nunca SQL.

## 1. Crear la base vacía
En SSMS, con tu cuenta autorizada, ejecuta `Infrastructure/Database/Scripts/000_create_database.sql`. No borra bases existentes. El nombre es **CoffeManager**.

## 2. Configurar conexión y activación
Abre PowerShell en la carpeta que contiene `CoffeManager.slnx` y ejecuta:

```powershell
.\Scripts\Configure-Local.ps1
```

Si tu instancia es predeterminada en vez de SQLEXPRESS:

```powershell
.\Scripts\Configure-Local.ps1 -SqlServer 'localhost'
```

El script configura secretos de desarrollo y muestra una clave de activación de un solo registro. Guárdala para la primera cuenta. No contiene claves predefinidas dentro del ZIP. No cambia permisos SQL ni instala SQL Server.

La conexión usa autenticación de Windows: el proceso de la API se conecta con tu usuario. Necesitas permiso para crear las tablas en esa base. `TrustServerCertificate=True` se configura exclusivamente para el SQL local de desarrollo; en producción se requiere una política de certificados propia. No se desactiva la validación HTTPS del cliente.

## 3. Inicializar tablas de forma controlada

```powershell
dotnet run --project CoffeManager.csproj --launch-profile https -- --initialize-database
```

El comando aplica las migraciones versionadas `001_initial.sql` y `002_complete_profiles.sql`, verifica los campos y termina; no crea personas ni sucursales. El esquema guarda su versión y no se recrea en cada login. No ejecutes esos scripts contra una base distinta con tablas homónimas; el esquema inicial requiere su propia base.

## 4. Levantar API y cliente
En una terminal:

```powershell
dotnet run --project CoffeManager.csproj --launch-profile https
```

Déjala abierta. La API escucha en **https://localhost:7066**. En Visual Studio selecciona `CoffeManager.Client.Admin` como proyecto de inicio y pulsa F5. También puedes abrir otra terminal:

```powershell
dotnet run --project CoffeManager.Client.Admin/CoffeManager.Client.Admin.csproj
```

En el cliente elige **Activar primera cuenta**, llena nombre, apellidos, correo, contraseña y la clave generada. Después inicia sesión. La activación se cierra cuando existe la primera cuenta. No permite elegir privilegios libremente.

## Qué está conectado

Consulta la matriz de campos y operaciones en `AUDITORIA.md`. Los perfiles completos y las asignaciones de cuenta/rol/sucursal están implementados. Suscripciones comerciales, áreas, inventario y Caja siguen pendientes.

Para probar el alta de sucursales en tu entorno local de desarrollo:

```powershell
dotnet user-secrets set 'DevelopmentFeatures:AllowUnlicensedBranchCreation' 'true' --project CoffeManager.csproj
```

Reinicia la API. Esta opción no activa un plan comercial ni define una cuota. Al terminar la prueba vuelve a `false`.

## Comprobar tu esquema real

```powershell
dotnet run --project CoffeManager.csproj --launch-profile https -- --verify-database
```

El verificador consulta tipos y longitudes de las columnas reales. No altera datos.

## Si no conecta
- No aparece el cliente: selecciona su proyecto de inicio, no el proyecto web.
- API inaccesible: verifica que esté abierta y que el certificado de desarrollo esté confiado.
- Error de BD: confirma instancia, base, permisos de tu usuario Windows y ejecución de inicialización.
- Cuenta inicial ya existe: inicia sesión; no se crea otro dueño por repetir activación.
- Demasiados intentos: espera un minuto. El límite local es de cinco intentos/minuto por IP para login/activación.
- Sesión terminada: inicia nuevamente. La cookie no se persiste a disco por el cliente y dura hasta ocho horas; no se ofrece sesión perpetua.

Puedes cambiar el origen de API mediante `COFFEMANAGER_API_URL`, siempre HTTPS. Esta entrega se prueba primero en localhost; no es una receta de exposición pública.

## Carpeta persistente de imágenes

En desarrollo se usa `App_Data/Media` dentro del servidor. Para elegir un disco/carpeta persistente en tu equipo Windows puedes configurar, por ejemplo:

```powershell
dotnet user-secrets set 'Media:RootPath' 'D:\CoffeManagerMedia' --project CoffeManager.csproj
```

Usa una carpeta donde el proceso de la API tenga permiso de escritura y respaldo. Mantén juntos el respaldo de SQL y esa carpeta: SQL guarda los identificadores, no el contenido de la fotografía.
