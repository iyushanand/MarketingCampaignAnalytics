using System;

namespace Backend.DTOs
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public decimal Income { get; set; }
        public string Education { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // RFM Metrics & Segmentation
        public int Recency { get; set; }
        public int Frequency { get; set; }
        public decimal Monetary { get; set; }
        public string RfmSegment { get; set; } = string.Empty; // "High Value", "Medium Value", "Low Value"
        public double ResponseRate { get; set; }
    }
}
