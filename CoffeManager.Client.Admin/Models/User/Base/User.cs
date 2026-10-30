using System.ComponentModel.DataAnnotations;
using CoffeManager.Client.Admin.Connection;
namespace CoffeManager.Client.Admin.Models.User.Base;
/// <summary>Owns user interaction structures and account/profile operations for this side of the connection.</summary>
internal abstract class User
{
    #region Operation Data
        public string _role { get; set; } = string.Empty;
        /// <summary>Reserves provider-controlled subscription information; null means not loaded or configured, never unlimited access.</summary>
        internal UserSubscriptionData? Subscription { get; private set; }
        protected readonly ApiConnection Connection;
    #endregion Operation Data
    #region Initialization
        /// <summary>Initializes the entity-specific communication dependencies.</summary>
        protected User(ApiConnection connection){Connection=connection;}
    #endregion Initialization
    #region Structs
        /// <summary>Contains the identity and activation credentials for the initial owner account. The server generates UserId.</summary>
        internal struct AccountRegistrationData
        {
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(80, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre(s)")]
            public string FirstName { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(80, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Apellido paterno")]
            public string PaternalLastName { get; set; }
            [StringLength(80, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Apellido materno")]
            public string? MaternalLastName { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), EmailAddress(ErrorMessage = "{0} debe ser un correo electrónico válido."), StringLength(254, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(128, MinimumLength = 12, ErrorMessage = "{0} debe tener entre {2} y {1} caracteres.")]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(200, MinimumLength = 24, ErrorMessage = "{0} debe tener entre {2} y {1} caracteres.")]
            [Display(Name = "Clave de activación")]
            public string SetupKey { get; set; }
        }
        /// <summary>Contains credentials submitted for authentication only; passwords never appear in query responses.</summary>
        internal struct LoginData
        {
            [Required(ErrorMessage = "{0} es obligatorio."), EmailAddress(ErrorMessage = "{0} debe ser un correo electrónico válido."), StringLength(254, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(128, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }
        }
        /// <summary>Contains the complete editable user profile. Optional null values clear previously stored values.</summary>
        internal struct UserProfileData
        {
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(100, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre para mostrar")]
            public string DisplayName { get; set; }
            [StringLength(1000, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Descripción")]
            public string? Description { get; set; }
            [Display(Name = "Foto de perfil")]
            public Guid? AvatarId { get; set; }
            [Display(Name = "Portada")]
            public Guid? CoverImageId { get; set; }
            [StringLength(30, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Teléfono")]
            public string? PhoneNumber { get; set; }
            [StringLength(30, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Teléfono alternativo")]
            public string? AlternatePhoneNumber { get; set; }
            [Display(Name = "Fecha de nacimiento")]
            public DateOnly? DateOfBirth { get; set; }
            [StringLength(250, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Calle, número y domicilio")]
            public string? Address { get; set; }
            [StringLength(100, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Ciudad")]
            public string? City { get; set; }
            [StringLength(100, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Estado")]
            public string? State { get; set; }
            [StringLength(10, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Código postal")]
            public string? PostalCode { get; set; }
            [StringLength(13, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "RFC")]
            public string? TaxIdentificationNumber { get; set; }
            [StringLength(18, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "CURP")]
            public string? NationalIdentificationNumber { get; set; }
        }
        /// <summary>Contains the identified user and complete profile without authentication secrets.</summary>
        internal struct UserDetailsData
        {
            public Guid UserId { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(80, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre(s)")]
            public string FirstName { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(80, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Apellido paterno")]
            public string PaternalLastName { get; set; }
            [StringLength(80, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Apellido materno")]
            public string? MaternalLastName { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), EmailAddress(ErrorMessage = "{0} debe ser un correo electrónico válido."), StringLength(254, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public bool IsOwner { get; set; }
            public UserProfileData Profile { get; set; }
        }
        /// <summary>Contains an identified catalog option returned by the server.</summary>
        internal struct CatalogData
        {
            public Guid Id { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(50, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            public string Code { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(100, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre")]
            public string Name { get; set; }
        }
        /// <summary>Contains the name of a new catalog option; identifiers are generated by the server.</summary>
        internal struct CatalogCreationData
        {
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(100, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre")]
            public string Name { get; set; }
        }
        /// <summary>Contains a staff account created by the owner before linking it to an employee.</summary>
        internal struct StaffAccountRegistrationData
        {
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(80, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre(s)")]
            public string FirstName { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(80, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Apellido paterno")]
            public string PaternalLastName { get; set; }
            [StringLength(80, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Apellido materno")]
            public string? MaternalLastName { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), EmailAddress(ErrorMessage = "{0} debe ser un correo electrónico válido."), StringLength(254, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(128, MinimumLength = 12, ErrorMessage = "{0} debe tener entre {2} y {1} caracteres.")]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }
        }
        /// <summary>Contains an unlinked staff account eligible for employee assignment; owner accounts are excluded.</summary>
        internal struct AvailableAccountData
        {
            [Display(Name = "Cuenta de acceso")]
            public Guid UserAccountId { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(100, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre para mostrar")]
            public string DisplayName { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), EmailAddress(ErrorMessage = "{0} debe ser un correo electrónico válido."), StringLength(254, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }
        }
        /// <summary>Contains server-controlled access capabilities for a role.</summary>
        internal struct RoleData
        {
            [Display(Name = "Rol")]
            public Guid RoleId { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(100, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre")]
            public string Name { get; set; }
            [Display(Name = "Permitir acceso a Administración")]
            public bool CanAccessAdministration { get; set; }
            [Display(Name = "Permitir acceso a Caja")]
            public bool CanAccessCheckout { get; set; }
        }
        /// <summary>Contains a role label and permissions explicitly configured by the owner.</summary>
        internal struct RoleCreationData
        {
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(100, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre")]
            public string Name { get; set; }
            [Display(Name = "Permitir acceso a Administración")]
            public bool CanAccessAdministration { get; set; }
            [Display(Name = "Permitir acceso a Caja")]
            public bool CanAccessCheckout { get; set; }
        }
        /// <summary>Contains provider-controlled subscription terms, outside editable registration and profile data.</summary>
        internal struct UserSubscriptionData
        {
            public Guid SubscriptionId { get; set; }
            public Guid UserId { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(80, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            public string SubscriptionName { get; set; }
            /// <summary>References optional special conditions, such as an agreed offer; not the service tier.</summary>
            public Guid? PlanId { get; set; }
            [StringLength(50, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            public string? PlanCode { get; set; }
            [StringLength(100, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            public string? PlanName { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(40, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            public string StatusCode { get; set; }
            public decimal MonthlyPrice { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(3, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            public string CurrencyCode { get; set; }
            public int? MaxBranches { get; set; }
            public int? MaxEmployees { get; set; }
            public long? StorageLimitBytes { get; set; }
            public DateTimeOffset StartsAt { get; set; }
            public DateTimeOffset? EndsAt { get; set; }
        }
    #endregion Structs
    #region Task Account Operations
        /// <summary>Sends the initial owner identity and activation credentials to the server.</summary>
        /// <param name="data">The initial registration fields.</param>
        /// <param name="cancellationToken">Cancels the network request.</param>
        /// <returns>The server-generated account identifier.</returns>
        internal Task<Guid> RegisterInitialAsync(AccountRegistrationData data,CancellationToken cancellationToken=default)=>Connection.CreateAsync("api/accounts/bootstrap",data,cancellationToken);
        /// <summary>Requests authentication; the connection retains the secure session cookie.</summary>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task LoginAsync(LoginData data,CancellationToken cancellationToken=default)=>Connection.SendAsync(HttpMethod.Post,"api/accounts/login",data,cancellationToken);
        /// <summary>Ends the authenticated session.</summary>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task LogoutAsync(CancellationToken cancellationToken=default)=>Connection.LogoutAsync(cancellationToken);
        /// <summary>Creates an unlinked staff account through the owner-authorized endpoint.</summary>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<Guid> RegisterStaffAsync(StaffAccountRegistrationData data,CancellationToken cancellationToken=default)=>Connection.CreateAsync("api/accounts/staff",data,cancellationToken);
        /// <summary>Returns staff accounts that have not been assigned to an employee.</summary>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal async Task<IReadOnlyList<AvailableAccountData>> GetAvailableAccountsAsync(CancellationToken cancellationToken=default)=>await Connection.GetAsync<List<AvailableAccountData>>("api/accounts/available",cancellationToken)??[];
    #endregion Task Account Operations
    #region Task Profile Operations
        /// <summary>Queries the authenticated user with the complete saved profile.</summary>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<UserDetailsData?> GetAsync(CancellationToken cancellationToken=default)=>Connection.GetAsync<UserDetailsData?>("api/users/me",cancellationToken);
        /// <summary>Sends the complete editable profile to the server.</summary>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task UpdateAsync(UserProfileData data,CancellationToken cancellationToken=default)=>Connection.SendAsync(HttpMethod.Put,"api/users/me",data,cancellationToken);
    #endregion Task Profile Operations
    #region Task Role Operations
        /// <summary>Returns persisted role options and their access capabilities.</summary>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal async Task<IReadOnlyList<RoleData>> GetRolesAsync(CancellationToken cancellationToken=default)=>await Connection.GetAsync<List<RoleData>>("api/roles",cancellationToken)??[];
        /// <summary>Requests creation of a role configured by the owner.</summary>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<Guid> AddRoleAsync(RoleCreationData data,CancellationToken cancellationToken=default){_role=data.Name;return Connection.CreateAsync("api/roles",data,cancellationToken);}
    #endregion Task Role Operations
}
