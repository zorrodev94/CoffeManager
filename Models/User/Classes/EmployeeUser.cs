using Microsoft.AspNetCore.Identity;
using CoffeManager.Models.User.Repositories;
using System.ComponentModel.DataAnnotations;
namespace CoffeManager.Models.User.Classes;
/// <summary>Extends user interactions with employee registration, profile and operational assignment.</summary>
internal abstract class EmployeeUser : Base.User
{
    #region Operation Data
        public string _salaryBasis { get; set; } = string.Empty;
        public string _employmentStatus { get; set; } = string.Empty;
        private readonly EmployeeRepository _employees;
    #endregion Operation Data
    #region Initialization
        protected EmployeeUser(EmployeeRepository employees,AccountRepository accounts,IPasswordHasher<Base.User> hasher):base(accounts,hasher){_employees=employees;}
    #endregion Initialization
    #region Obsolete Arrays
        /// <summary>Contains salary bases used once during controlled catalog initialization, never by routine account operations.</summary>
        [Obsolete("This array is used only once during initialization.")]
        private static readonly string[] DefaultSalaryBases=["Hourly","Daily","Weekly","Biweekly","Semimonthly","Monthly"];
        /// <summary>Contains employment statuses used once during controlled catalog initialization, then read from SQL.</summary>
        [Obsolete("This array is used only once during initialization.")]
        private static readonly string[] DefaultEmploymentStatuses=["Pending","Active","OnLeave","Suspended","Ended"];
    #endregion Obsolete Arrays
    #region Task Database Initialization
        /// <summary>Supplies the defaults exclusively to the guarded schema initializer.</summary>
        internal static (string[] Salary,string[] Statuses) GetInitializationCatalogs()
        {
            #pragma warning disable CS0618
            return (DefaultSalaryBases,DefaultEmploymentStatuses);
            #pragma warning restore CS0618
        }
    #endregion Task Database Initialization
    #region Structs
        /// <summary>Contains the minimal identity submitted when adding an employee; no account is created implicitly.</summary>
        internal struct EmployeeCreationData
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
        }
        /// <summary>Contains additional personal and employment fields completed after minimal creation.</summary>
        internal struct EmployeeProfileData
        {
            [EmailAddress(ErrorMessage = "{0} debe ser un correo electrónico válido."), StringLength(254, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Correo electrónico")]
            public string? Email { get; set; }
            [StringLength(100, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre para mostrar")]
            public string? DisplayName { get; set; }
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
            [StringLength(30, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Número de empleado")]
            public string? EmployeeNumber { get; set; }
            [Display(Name = "Fecha de contratación")]
            public DateOnly? HireDate { get; set; }
            [Display(Name = "Estado laboral")]
            public Guid? EmploymentStatusId { get; set; }
            [Range(typeof(decimal), "0", "9999999999999999.99", ErrorMessage = "{0} debe ser un importe válido mayor o igual que cero.")]
            [Display(Name = "Salario (MXN)")]
            public decimal? SalaryAmount { get; set; }
            [Display(Name = "Periodicidad del salario")]
            public Guid? SalaryBasisId { get; set; }
        }
        /// <summary>Contains branch, role and optional account assignment. The server validates ownership and required access atomically.</summary>
        internal struct EmployeeAssignmentData
        {
            [Display(Name = "Sucursal")]
            public Guid? BranchId { get; set; }
            [Display(Name = "Rol")]
            public Guid? RoleId { get; set; }
            [Display(Name = "Cuenta de acceso")]
            public Guid? UserAccountId { get; set; }
        }
        /// <summary>Contains identity, profile and current operational assignment of an employee.</summary>
        internal struct EmployeeDetailsData
        {
            public Guid EmployeeId { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public EmployeeCreationData Identity { get; set; }
            public EmployeeProfileData Profile { get; set; }
            public EmployeeAssignmentData Assignment { get; set; }
        }
    #endregion Structs
    #region Task Employee Operations
        /// <summary>Queries employee identities, complete profiles and assignments within the owner scope.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<IReadOnlyList<EmployeeDetailsData>> GetEmployeesAsync(Guid ownerId,CancellationToken cancellationToken=default)=>_employees.GetAsync(ownerId,cancellationToken);
        /// <summary>Creates only the minimal employee identity.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<Guid> RegisterEmployeeAsync(Guid ownerId,EmployeeCreationData data,CancellationToken cancellationToken=default)=>_employees.CreateAsync(ownerId,data,cancellationToken);
        /// <summary>Persists additional personal and employment fields.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="employeeId">The selected employee identifier.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task UpdateEmployeeAsync(Guid ownerId,Guid employeeId,EmployeeProfileData data,CancellationToken cancellationToken=default)=>_employees.UpdateAsync(ownerId,employeeId,data,cancellationToken);
        /// <summary>Validates ownership, role requirements and account uniqueness before assigning an employee.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="employeeId">The selected employee identifier.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task AssignEmployeeAsync(Guid ownerId,Guid employeeId,EmployeeAssignmentData data,CancellationToken cancellationToken=default)=>_employees.AssignAsync(ownerId,employeeId,data,cancellationToken);
        /// <summary>Queries the salary basis or employment status catalog.</summary>
        /// <param name="catalog">The approved salary-bases or employment-statuses catalog key.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<IReadOnlyList<CatalogData>> GetCatalogAsync(string catalog,CancellationToken cancellationToken=default)=>_employees.GetCatalogAsync(catalog,cancellationToken);
        /// <summary>Creates one persisted option in an approved employment catalog.</summary>
        /// <param name="catalog">The approved salary-bases or employment-statuses catalog key.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<Guid> AddCatalogOptionAsync(string catalog,CatalogCreationData data,CancellationToken cancellationToken=default){if(catalog=="salary-bases")_salaryBasis=data.Name;else if(catalog=="employment-statuses")_employmentStatus=data.Name;return _employees.AddCatalogOptionAsync(catalog,data,cancellationToken);}
    #endregion Task Employee Operations
}
