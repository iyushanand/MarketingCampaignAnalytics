using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// Represents the response of a customer to a campaign.
    /// </summary>
    public class CampaignResponse
    {
        /// <summary>
        /// Gets or sets the primary key for the campaign response.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResponseId { get; set; }

        /// <summary>
        /// Gets or sets the customer ID.
        /// </summary>
        [Required]
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the associated Customer entity.
        /// </summary>
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        /// <summary>
        /// Gets or sets the campaign ID.
        /// </summary>
        [Required]
        public int CampaignId { get; set; }

        /// <summary>
        /// Gets or sets the associated Campaign entity.
        /// </summary>
        [ForeignKey("CampaignId")]
        public Campaign? Campaign { get; set; }

        /// <summary>
        /// Gets or sets the response (e.g., Yes, No).
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Response { get; set; } = "No";

        /// <summary>
        /// Gets or sets the total purchase amount generated.
        /// </summary>
        [Range(0, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchaseAmount { get; set; }

        /// <summary>
        /// Gets or sets the date of the purchase.
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime PurchaseDate { get; set; }

        /// <summary>
        /// Gets or sets the number of purchases made by the customer during this campaign.
        /// </summary>
        [Range(0, 1000)]
        public int NumberOfPurchases { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the response record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
