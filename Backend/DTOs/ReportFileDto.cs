using System;

namespace Backend.DTOs
{
    /// <summary>
    /// Represents a generated report file's metadata.
    /// </summary>
    public class ReportFileDto
    {
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file format/type (e.g., Excel, PDF).
        /// </summary>
        public string FileType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the formatted size of the file (e.g., "12.5 KB").
        /// </summary>
        public string FileSize { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the report was generated.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the URL to trigger the file download.
        /// </summary>
        public string DownloadUrl { get; set; } = string.Empty;
    }
}
