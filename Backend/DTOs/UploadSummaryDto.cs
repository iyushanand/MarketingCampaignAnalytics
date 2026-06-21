namespace Backend.DTOs
{
    public class UploadSummaryDto
    {
        public int CustomersImported { get; set; }
        public int CampaignsImported { get; set; }
        public int ResponsesImported { get; set; }
        public string Status { get; set; } = "Success";
        public string Message { get; set; } = string.Empty;
    }
}
