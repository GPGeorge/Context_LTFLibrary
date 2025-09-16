namespace LTF_Library_V1.Data.Models
{
    public class WhoAmIResult
    {
        public bool Success
        {
            get; set;
        }
        public string? Username
        {
            get; set;
        }
        public ClaimInfo[]? Claims
        {
            get; set;
        }
    }

    public class ClaimInfo
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
