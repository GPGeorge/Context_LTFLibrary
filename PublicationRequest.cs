using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTF_Library_V1.Data.Models
{
    [Table("tblPublicationRequests")]
    public class PublicationRequest
    {
        [Key]
        public int RequestID
        {
            get; set;
        }

        public int PublicationID
        {
            get; set;
        }

        [Required]
        [StringLength(10)]
        public string RequestType { get; set; } = "Borrow";

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone
        {
            get; set;
        }

        [Required]
        [StringLength(100)]
        public string ResearchPurpose { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? AdditionalInfo
        {
            get; set;
        }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

       
        public string? ProcessedBy
        {
            get; set;
        }  

        public DateTime? ProcessedDate
        {
            get; set;
        }

        [StringLength(1000)]
        public string? AdminNotes
        {
            get; set;
        }

        // Navigation properties
        [ForeignKey("PublicationID")]
        public virtual Publication? Publication
        {
            get; set;
        }

        [ForeignKey("ProcessedBy")]
        public virtual ApplicationUser? ProcessedByUser
        {
            get; set;
        }
    }

}
