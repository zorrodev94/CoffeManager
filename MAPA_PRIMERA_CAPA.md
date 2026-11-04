# Mapa de estudio · primera capa

Este documento no sustituye tu lectura anterior: identifica qué cambió para seguir estudiando sobre esta entrega.

## Arranque y presentación

- `CoffeManager.Client.Admin/Program.cs`: crea ApiConnection, AdminSession y AccessForm.
- `Presentation/Windows/ObsidianAccessForm.cs`: convierte el tamaño inicial según DeviceDpi y organiza formulario, fotografía y pie.
- `Presentation/Theme/AccessLayoutMetrics.cs`: calcula tamaño inicial y regiones sin depender de SQL.
- `Presentation/Controls/AccessStackPanel.cs`: mide texto y dispone controles; adapta los pares de campos al ancho.
- `Presentation/Controls/ObsidianField.cs`: TextBox nativo y botón de visibilidad.
- `Presentation/Controls/PasswordVisibilityButton.cs`: dibujo local del ojo.

## Primera activación

`AccessForm.RefreshSetupAsync` → cliente User.IsSetupRequiredAsync → GET api/accounts/setup-required → AccountRepository.IsSetupRequiredAsync.

Cuando corresponde: `AccessForm.RegisterAsync` → AccountActivationForm → cliente User.RegisterInitialAsync → ApiConnection.CreateAsync → POST api/accounts/bootstrap → RequestValidation → comprobación de Setup:Key → servidor User.RegisterInitialAsync → hash → AccountRepository.CreateOwnerAsync → SQL.

No se recrean tablas desde el formulario. La preparación del esquema permanece en el comando explícito de inicialización ya estudiado.

## Login y sesión

`AccessForm.LoginAsync` → cliente User.LoginAsync → ApiConnection.SendAsync → POST api/accounts/login → servidor User.AuthenticateAsync → AccountRepository.FindLoginAsync → verificación del hash → GetAccessAsync → SignInAsync → cookie → apertura de AdminForm.

`ApiConnection.LogoutAsync` ahora expira también las cookies locales en un finally, incluso cuando el servidor no responde. Eso limpia la sesión local; no constituye una revocación remota de una copia de la cookie que pudiera existir fuera de ese cliente.

## Registro desde Acceso

`AccessForm.RegisterStaffAsync` abre OwnerAuthorizationForm. Este usa LoginData y los métodos existentes de login y consulta del usuario; solo continúa si IsOwner es true. Luego solicita los catálogos y abre StaffAccountForm. El finally de AccessForm cierra esa sesión temporal al salir.

StaffAccountForm usa la estructura espejo StaffAccountRegistrationData. Los identificadores opcionales de vinculación ya existían en la entrega Responsive y no se cambiaron aquí. Vincular ahora es una decisión explícita de presentación, no un campo adicional persistido.

La cuenta se guarda mediante RegisterStaffAsync → api/accounts/staff → AccountRepository.CreateStaffAsync. La autorización y validación del servidor siguen vigentes aunque se intente invocar la ruta por fuera de la ventana.

## Perfil y empleado

No se adelantó el perfil completo ni la siguiente capa de permisos. Las estructuras de cuenta, login, perfil y empleado continúan separadas por interacción. UserAccountId mantiene la vinculación persistente; no se usa el nombre para unir personas automáticamente.
