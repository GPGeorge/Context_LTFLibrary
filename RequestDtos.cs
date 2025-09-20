// DTOs/RequestDtos.cs
using System.ComponentModel.DataAnnotations;

namespace LTF_Library_V1.DTOs
{
    public class PendingRequestDto
    {
        public int RequestID { get; set; }
        public string PublicationTitle { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string ResearchPurpose { get; set; } = string.Empty;
        public string? AdditionalInfo { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string RequestType { get; set; } = string.Empty;
        
        // Computed properties for display
        public string RequesterName => $"{FirstName} {LastName}";
        public string FormattedRequestDate => RequestDate.ToString("MMM dd, yyyy");
        public int DaysAgo => (DateTime.Now - RequestDate).Days;
    }

    public class ProcessRequestDto
    {
        [Required]
        public int RequestID { get; set; }
        
        [Required]
        public string Action { get; set; } = string.Empty; // "Approve", "Deny", "RequestInfo"
        
        public string? AdminNotes { get; set; }
        
        [Required]
        public string ProcessedBy { get; set; } = string.Empty;
    }

    public class RequestProcessingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }

    public class RequestStatisticsDto
    {
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int DeniedCount { get; set; }
        public int TotalCount { get; set; }
        public double AverageProcessingDays { get; set; }
    }

    public class RequestEmailDto
    {
        public string ToEmail { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public string PublicationTitle { get; set; } = string.Empty;
    }
}