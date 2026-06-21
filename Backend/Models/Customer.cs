using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }

        public int Age { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Income { get; set; }

        [Required]
        [MaxLength(50)]
        public string Education { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string MaritalStatus { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Country { get; set; } = string.Empty;
    }
}
