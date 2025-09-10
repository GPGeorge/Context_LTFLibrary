namespace LTF_Library_V1.Data.Models
{
    public class PendingPublicRequest
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
    }
}
