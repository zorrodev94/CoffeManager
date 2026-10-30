using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using CoffeManager.Models.User.Repositories;
namespace CoffeManager.Models.User.Base;
/// <summary>Owns user interaction structures and account/profile operations for this side of the connection.</summary>
internal abstract class User
{
    #region Operation Data
        public string _role { get; set; } = string.Empty;
        /// <summary>Reserves provider-controlled subscription information; null means not loaded or configured, never unlimited access.</summary>
        internal UserSubscriptionData? Subscription { get; private set; }
        private readonly AccountRepository _repository;
        private readonly IPasswordHasher<User> _hasher;
        private static readonly string DummyHash=new PasswordHasher<User>().HashPassword(null!,"NonCustomerDummyPassword-2026");
    #endregion Operation Data
    #region Initialization
        /// <summary>Initializes the entity-specific communication dependencies.</summary>
        protected User(AccountRepository repository,IPasswordHasher<User> hasher){_repository=repository;_hasher=hasher;}
    #endregion Initialization
    #region Obsolete Arrays
        /// <summary>Contains the roles used once during controlled catalog initialization. Subsequent operations read SQL catalogs.</summary>
        [Obsolete("This array is used only once during initialization.")]
        private static readonly string[] UserRoles=["Admin","Manager","Employee","Cashier"];
    #endregion Obsolete Arrays
    #region Task Database Initialization
        /// <summary>Supplies defaults exclusively to the guarded database initializer.</summary>
        internal static string[] GetInitializationRoles()
        {
            #pragma warning disable CS0618
            return UserRoles;
            #pragma warning restore CS0618
        }
    #endregion Task Database Initialization
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
        /// <summary>Hashes the initial password and atomically registers the first owner.</summary>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<Guid?> RegisterInitialAsync(AccountRegistrationData data,CancellationToken cancellationToken=default)=>_repository.CreateOwnerAsync(data,_hasher.HashPassword(this,data.Password),cancellationToken);
        /// <summary>Verifies the password without exposing authentication material to the client.</summary>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal async Task<Guid?> AuthenticateAsync(LoginData data,CancellationToken cancellationToken=default)
        {
            var account=await _repository.FindLoginAsync(data.Email,cancellationToken);
            if(account is null){_hasher.VerifyHashedPassword(this,DummyHash,data.Password);return null;}
            return _hasher.VerifyHashedPassword(this,account.Value.Hash,data.Password)==PasswordVerificationResult.Failed?null:account.Value.Id;
        }
        /// <summary>Creates an unlinked staff account without granting operational access.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<Guid> RegisterStaffAsync(Guid ownerId,StaffAccountRegistrationData data,CancellationToken cancellationToken=default)=>_repository.CreateStaffAsync(ownerId,data,_hasher.HashPassword(this,data.Password),cancellationToken);
        /// <summary>Queries eligible accounts, excluding owners and already linked accounts.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<IReadOnlyList<AvailableAccountData>> GetAvailableAccountsAsync(Guid ownerId,CancellationToken cancellationToken=default)=>_repository.GetAvailableAccountsAsync(ownerId,cancellationToken);
    #endregion Task Account Operations
    #region Task Profile Operations
        /// <summary>Loads the requested authenticated account and its complete profile.</summary>
        /// <param name="userId">The authenticated user identifier.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<UserDetailsData?> GetAsync(Guid userId,CancellationToken cancellationToken=default)=>_repository.GetAsync(userId,cancellationToken);
        /// <summary>Persists the complete editable profile without changing login identity or permissions.</summary>
        /// <param name="userId">The authenticated user identifier.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task UpdateAsync(Guid userId,UserProfileData data,CancellationToken cancellationToken=default)=>_repository.UpdateProfileAsync(userId,data,cancellationToken);
    #endregion Task Profile Operations
    #region Task Role Operations
        /// <summary>Queries persisted roles and server-enforced access capabilities.</summary>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<IReadOnlyList<RoleData>> GetRolesAsync(CancellationToken cancellationToken=default)=>_repository.GetRolesAsync(cancellationToken);
        /// <summary>Creates a persisted role; only an owner-authorized endpoint invokes this operation.</summary>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<Guid> AddRoleAsync(RoleCreationData data,CancellationToken cancellationToken=default){_role=data.Name;return _repository.AddRoleAsync(data,cancellationToken);}
    #endregion Task Role Operations
}
