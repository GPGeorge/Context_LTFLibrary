using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTF_Library_V1.Data.Models
{
    [Table("tblPublicationTransfer")]
    public class PublicationTransfer
    {
        [Key]
        public int PublicationTransferID
        {
            get; set;
        }

        public int? ParticipantID
        {
            get; set;
        }

        public int? PublicationID
        {
            get; set;
        }

        public int? ParticipantStatusID
        {
            get; set;
        }

        [Column(TypeName = "money")]
        public decimal? EstimatedValue
        {
            get; set;
        }

        public DateTime? TransferDate
        {
            get; set;
        }



        // Navigation properties
        [ForeignKey("PublicationID")]
        public virtual Publication? Publication
        {
            get; set;
        }

        [ForeignKey("ParticipantID")]
        public virtual Participant? Participant
        {
            get; set;
        }

        [ForeignKey("ParticipantStatusID")]
        public virtual ParticipantStatus? ParticipantStatus
        {
            get; set;
        }
    }
}