using System.ComponentModel.DataAnnotations;
using CoffeManager.Client.Admin.Connection;
namespace CoffeManager.Client.Admin.Models.Business.Classes;
/// <summary>Owns branch interaction structures and entity-specific operations.</summary>
internal sealed class Branch(ApiConnection connection)
{
    #region Structs
        /// <summary>Contains only the selected business and initial branch name; server validates ownership and capacity.</summary>
        internal struct BranchCreationData
        {
            [Display(Name = "Negocio")]
            public Guid BusinessId { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(120, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre")]
            public string Name { get; set; }
        }
        /// <summary>Contains the complete editable branch profile without moving it to another business.</summary>
        internal struct BranchProfileData
        {
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(120, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre")]
            public string Name { get; set; }
            [StringLength(1000, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Descripción")]
            public string? Description { get; set; }
            [StringLength(250, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Calle, número y domicilio")]
            public string? Address { get; set; }
            [StringLength(100, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Colonia")]
            public string? Neighborhood { get; set; }
            [StringLength(100, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Ciudad")]
            public string? City { get; set; }
            [StringLength(100, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Estado")]
            public string? State { get; set; }
            [StringLength(10, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Código postal")]
            public string? PostalCode { get; set; }
            [StringLength(30, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Teléfono")]
            public string? PhoneNumber { get; set; }
            [EmailAddress(ErrorMessage = "{0} debe ser un correo electrónico válido."), StringLength(254, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Correo electrónico")]
            public string? Email { get; set; }
            [Display(Name = "Logo")]
            public Guid? LogoId { get; set; }
            [Display(Name = "Portada")]
            public Guid? CoverImageId { get; set; }
        }
        /// <summary>Contains an identified branch, its display name and complete profile.</summary>
        internal struct BranchDetailsData
        {
            [Display(Name = "Sucursal")]
            public Guid BranchId { get; set; }
            [Display(Name = "Negocio")]
            public Guid BusinessId { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(120, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre")]
            public string Name { get; set; }
            public BranchProfileData Profile { get; set; }
        }
    #endregion Structs
    #region Task Branch Operations
        /// <summary>Queries branches with their complete profiles.</summary>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal async Task<IReadOnlyList<BranchDetailsData>> GetAsync(CancellationToken cancellationToken=default)=>await connection.GetAsync<List<BranchDetailsData>>("api/branches",cancellationToken)??[];
        /// <summary>Registers a branch within the selected authorized business.</summary>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<Guid> RegisterAsync(BranchCreationData data,CancellationToken cancellationToken=default)=>connection.CreateAsync("api/branches",data,cancellationToken);
        /// <summary>Updates a branch profile without changing its business relationship.</summary>
        /// <param name="branchId">The selected branch identifier.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task UpdateAsync(Guid branchId,BranchProfileData data,CancellationToken cancellationToken=default)=>connection.SendAsync(HttpMethod.Put,$"api/branches/{branchId}/profile",data,cancellationToken);
    #endregion Task Branch Operations
}
