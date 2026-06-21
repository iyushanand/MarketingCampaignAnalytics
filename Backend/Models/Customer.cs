using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// Represents a customer in the marketing database.
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Gets or sets the primary key for the customer.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the first name of the customer.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the customer.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the gender of the customer.
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the age of the customer.
        /// </summary>
        [Range(0, 150)]
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the annual income of the customer.
        /// </summary>
        [Range(0, 100000000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Income { get; set; }

        /// <summary>
        /// Gets or sets the education level of the customer.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Education { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the marital status of the customer.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string MaritalStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the country of residence.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the city of residence.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the customer record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property for campaign responses.
        /// </summary>
        public ICollection<CampaignResponse> CampaignResponses { get; set; } = new List<CampaignResponse>();
    }
}
