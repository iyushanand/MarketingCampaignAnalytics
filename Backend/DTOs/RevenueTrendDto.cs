namespace Backend.DTOs
{
    public class RevenueTrendDto
    {
        public string Month { get; set; } = string.Empty; // e.g. "2026-01" or "January 2026"
        public decimal Revenue { get; set; }
    }
}
