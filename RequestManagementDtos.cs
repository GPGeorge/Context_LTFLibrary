// DTOs/RequestManagementDtos.cs
using System.ComponentModel.DataAnnotations;

namespace LTF_Library_V1.DTOs
{
    public class PendingRequestDto
    {
        public int RequestID
        {
            get; set;
        }
        public string PublicationTitle { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone
        {
            get; set;
        }
        public string ResearchPurpose { get; set; } = string.Empty;
        public string? AdditionalInfo
        {
            get; set;
        }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestDate
        {
            get; set;
        }
        public string RequestType { get; set; } = string.Empty;

        // Computed properties for display
        public string RequesterName => $"{FirstName} {LastName}";
        public string FormattedRequestDate => RequestDate.ToString("MMM dd, yyyy h:mm tt");
        public int DaysAgo => ( DateTime.Now - RequestDate ).Days;
    }

    public class ProcessRequestDto
    {
        [Required]
        public int RequestID
        {
            get; set;
        }

        [Required(ErrorMessage = "Please select an action")]
        public string Action { get; set; } = string.Empty; // "Approve", "Deny", "RequestInfo"

        [StringLength(1000)]
        public string? AdminNotes
        {
            get; set;
        }

        public string ProcessedBy { get; set; } = string.Empty;
    }

    public class RequestProcessingResult
    {
        public bool Success
        {
            get; set;
        }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }
}