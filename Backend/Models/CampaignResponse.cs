using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class CampaignResponse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResponseId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Required]
        public int CampaignId { get; set; }

        [ForeignKey("CampaignId")]
        public Campaign? Campaign { get; set; }

        [Required]
        [MaxLength(10)]
        public string Response { get; set; } = string.Empty; // "Yes" or "No"

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchaseAmount { get; set; }

        public DateTime PurchaseDate { get; set; }
    }
}
