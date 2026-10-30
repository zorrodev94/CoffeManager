using System.ComponentModel.DataAnnotations;
using CoffeManager.Models.Business.Repositories;
namespace CoffeManager.Models.Business.Classes;
/// <summary>Owns business interaction structures and entity-specific operations.</summary>
internal sealed class Business(BusinessRepository repository)
{
    #region Structs
        /// <summary>Contains the minimal business name and supported type; ownership comes from the authenticated session.</summary>
        internal struct BusinessCreationData
        {
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(120, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre para mostrar")]
            public string TradeName { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(40, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Giro")]
            public string BusinessType { get; set; }
        }
        /// <summary>Contains the complete editable business profile without changing its owner or business type.</summary>
        internal struct BusinessProfileData
        {
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(120, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre para mostrar")]
            public string TradeName { get; set; }
            [StringLength(200, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Nombre legal")]
            public string? LegalName { get; set; }
            [StringLength(1000, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Descripción")]
            public string? Description { get; set; }
            [Display(Name = "Logo")]
            public Guid? LogoId { get; set; }
            [Display(Name = "Portada")]
            public Guid? CoverImageId { get; set; }
            [StringLength(13, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "RFC")]
            public string? TaxIdentificationNumber { get; set; }
            [StringLength(30, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Teléfono")]
            public string? PhoneNumber { get; set; }
            [EmailAddress(ErrorMessage = "{0} debe ser un correo electrónico válido."), StringLength(254, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Correo electrónico")]
            public string? Email { get; set; }
        }
        /// <summary>Contains the identified business and its complete profile.</summary>
        internal struct BusinessDetailsData
        {
            [Display(Name = "Negocio")]
            public Guid BusinessId { get; set; }
            public Guid OwnerUserId { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            [Required(ErrorMessage = "{0} es obligatorio."), StringLength(40, ErrorMessage = "{0} debe tener como máximo {1} caracteres.")]
            [Display(Name = "Giro")]
            public string BusinessType { get; set; }
            public BusinessProfileData Profile { get; set; }
        }
    #endregion Structs
    #region Task Business Operations
        /// <summary>Queries the current business and its complete profile.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<BusinessDetailsData?> GetAsync(Guid ownerId,CancellationToken cancellationToken=default)=>repository.GetAsync(ownerId,cancellationToken);
        /// <summary>Creates the minimal business with the supported cafeteria type.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task<Guid> RegisterAsync(Guid ownerId,BusinessCreationData data,CancellationToken cancellationToken=default)=>repository.CreateAsync(ownerId,data,cancellationToken);
        /// <summary>Saves the complete business profile.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal Task UpdateAsync(Guid ownerId,BusinessProfileData data,CancellationToken cancellationToken=default)=>repository.UpdateAsync(ownerId,data,cancellationToken);
    #endregion Task Business Operations
}
