using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Campaign
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CampaignId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CampaignName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Channel { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Budget { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Spend { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Revenue { get; set; }

        public int Conversions { get; set; }

        public int Clicks { get; set; }

        public int Impressions { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
