using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTF_Library_V1.Data.Models
{
    [Table("tblParticipant")]
    public class Participant
    {
        [Key]
        public int ParticipantID
        {
            get; set;
        }

        [StringLength(50)]
        public string? ParticipantFirstName
        {
            get; set;
        }

        [StringLength(50)]
        public string? ParticipantLastName
        {
            get; set;
        }

        [StringLength(50)]
        public string? AlsoKnownAs
        {
            get; set;
        }

        // Navigation property - if you have PublicationTransfer records
        public virtual ICollection<PublicationTransfer>? PublicationTransfers
        {
            get; set;
        }
    }
}