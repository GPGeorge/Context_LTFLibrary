
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LTF_Library_V1.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FirstName
        {
            get; set;
        }

        [StringLength(100)]
        public string? LastName
        {
            get; set;
        }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastLoginDate
        {
            get; set;
        }

        public bool IsActive { get; set; } = true;

        // Navigation property for processed requests
        public virtual ICollection<PublicationRequest> ProcessedRequests { get; set; } = new List<PublicationRequest>();

        // Computed property for display
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}