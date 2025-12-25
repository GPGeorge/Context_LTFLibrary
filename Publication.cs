using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTF_Library_V1.Data.Models
{
    [Table("tblPublication")]
    public class Publication
    {
        [Key]
        public int PublicationID
        {
            get; set;
        }

        [Required]
        [MaxLength(1000)]
        public string PublicationTitle { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? CatalogNumber
        {
            get; set;
        }

        public string? Comments
        {
            get; set;
        }

        [MaxLength(255)]
        public string? CoverPhotoLink
        {
            get; set;
        }

        [MaxLength(255)]
        public string? Edition
        {
            get; set;
        }

        [MaxLength(50)]
        public string? ISBN
        {
            get; set;
        }

        public int? MediaConditionID
        {
            get; set;
        }
        public int? MediaTypeID
        {
            get; set;
        }
        public int? Volume
        {
            get; set;
        }
        public int? NumberOfVolumes
        {
            get; set;
        }
        public int? Pages
        {
            get; set;
        }

        [MaxLength(255)]
        public string? Printing
        {
            get; set;
        }

        public int? PublisherID
        {
            get; set;
        }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(50)]
        public string? PubSort
        {
            get; set;
        }

        [MaxLength(25)]
        public string? YearPublished
        {
            get; set;
        }

        public int? ConfidenceLevel
        {
            get; set;
        }
        public DateTime? DateCaptured
        {
            get; set;
        }

        [Column("InternalComments")]
        public string? InternalComments
        {
            get; set;
        }

        public int? ShelfID
        {
            get; set;
        }

        [Column(TypeName = "money")]
        public decimal? ListPrice
        {
            get; set;
        }

        // Navigation properties
        public virtual MediaCondition? MediaCondition
        {
            get; set;
        }
        public virtual MediaType? MediaType
        {
            get; set;
        }
        public virtual Publisher? Publisher
        {
            get; set;
        }
        public virtual Shelf? Shelf
        {
            get; set;
        }
        public virtual ICollection<PublicationCreator> PublicationCreators { get; set; } = new List<PublicationCreator>();
        public virtual ICollection<PublicationGenre> PublicationGenres { get; set; } = new List<PublicationGenre>();
        public virtual ICollection<PublicationKeyWord> PublicationKeyWords { get; set; } = new List<PublicationKeyWord>();
    }

    [Table("tblCreator")]
    public class Creator
    {
        [Key]
        public int CreatorID
        {
            get; set;
        }

        [MaxLength(100)]
        public string? CreatorFirstName
        {
            get; set;
        }

        [MaxLength(100)]
        public string? CreatorMiddleName
        {
            get; set;
        }

        [MaxLength(100)]
        public string? CreatorLastName
        {
            get; set;
        }

        public virtual ICollection<PublicationCreator> PublicationCreators { get; set; } = new List<PublicationCreator>();

        [NotMapped]
        public string FullName => $"{CreatorFirstName} {CreatorMiddleName} {CreatorLastName}".Trim().Replace("  ", " ");

        [NotMapped]
        public string SortName => $"{CreatorLastName}, {CreatorFirstName} {CreatorMiddleName}".Trim().Replace("  ", " ");
    }

    [Table("tblPublisher")]
    public class Publisher
    {
        [Key]
        public int PublisherID
        {
            get; set;
        }

        [MaxLength(255)]
        public string? Publisher1
        {
            get; set;
        }

        [MaxLength(255)]
        public string? PublisherGoogle
        {
            get; set;
        }

        public virtual ICollection<Publication> Publications { get; set; } = new List<Publication>();

        [NotMapped]
        public string DisplayName =>
            !string.IsNullOrEmpty(PublisherGoogle) && Publisher1 != PublisherGoogle
                ? $"{Publisher1} ({PublisherGoogle})"
                : Publisher1 ?? string.Empty;
    }

    [Table("tblGenre")]
    public class Genre
    {
        [Key]
        public int GenreID
        {
            get; set;
        }

        [MaxLength(255)]
        public string? Genre1
        {
            get; set;
        }

        public int? SortOrder
        {
            get; set;
        }

        public virtual ICollection<PublicationGenre> PublicationGenres { get; set; } = new List<PublicationGenre>();
    }

    [Table("tblMediaType")]
    public class MediaType
    {
        [Key]
        public int MediaTypeID
        {
            get; set;
        }

        [MaxLength(255)]
        public string? MediaType1
        {
            get; set;
        }

        public int? SortOrder
        {
            get; set;
        }

        public virtual ICollection<Publication> Publications { get; set; } = new List<Publication>();
    }

    [Table("tblMediaCondition")]
    public class MediaCondition
    {
        [Key]
        public int MediaConditionID
        {
            get; set;
        }

        [MaxLength(255)]
        public string? MediaCondition1
        {
            get; set;
        }

        public int? SortOrder
        {
            get; set;
        }

        public virtual ICollection<Publication> Publications { get; set; } = new List<Publication>();
    }

    [Table("tblParticipantStatus")]
    public class ParticipantStatus
    {
        [Column("ParticipantStatusID")]
        [Key]
        public int ParticipantStatusID
        {
            get; set;
        }
        [Column("ParticipantStatus")] 
        [MaxLength(255)]
        public string? ParticipantStatus1
        {
            get; set;
        }
        [Column("ExtendedDescription")]
        [MaxLength(100)]
        public string? ExtendedDescription
        {
            get; set;
        }
        [Column("TransactionType")]
        [MaxLength(10)]
        public string? TransactionType
        {
            get; set;
        }
        public int? SortOrder
        {
            get; set;
        }
        public virtual ICollection<PublicationTransfer>? PublicationTransfers
        {
            get; set;
        }
    }


    [Table("tblBookcase")]
    public class Bookcase
    {
        [Key]
        public int BookcaseID
        {
            get; set;
        }

        [MaxLength(50)]
        public string Bookcase1
        {
            get; set;
        }

        [MaxLength(150)]
        public string? BookcaseDescription
        {
            get; set;
        }

        public virtual ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
    }

    [Table("tblShelf")]
    public class Shelf
    {
        [Key]
        public int ShelfID
        {
            get; set;
        }

        [MaxLength(50)]
        public string? Shelf1
        {
            get; set;
        }

        [MaxLength(100)]
        public string? ShelfDescription
        {
            get; set;
        }

        public int? BookCaseID
        {
            get; set;
        }

        public virtual Bookcase? Bookcase
        {
            get; set;
        }
        public virtual ICollection<Publication> Publications { get; set; } = new List<Publication>();

        [NotMapped]
        public string Location => $"{Bookcase?.Bookcase1}-{Shelf1}";
    }

    [Table("tblPublicationCreator")]
    public class PublicationCreator
    {
        [Key]
        public int CreatorPublicationID
        {
            get; set;
        }

        public int? CreatorID
        {
            get; set;
        }
        public int? PublicationID
        {
            get; set;
        }

        public virtual Creator? Creator
        {
            get; set;
        }
        public virtual Publication? Publication
        {
            get; set;
        }
    }

    [Table("tblPublicationGenre")]
    public class PublicationGenre
    {
        [Key]
        public int PublicationGenreID
        {
            get; set;
        }

        public int? PublicationID
        {
            get; set;
        }
        public int? GenreID
        {
            get; set;
        }

        public virtual Genre? Genre
        {
            get; set;
        }
        public virtual Publication? Publication
        {
            get; set;
        }
    }

    [Table("tblPublicationKeyWord")]
    public class PublicationKeyWord
    {
        [Key]
        public int PublicationKeyWordID
        {
            get; set;
        }

        public int? PublicationID
        {
            get; set;
        }

        [MaxLength(255)]
        public string? KeyWord
        {
            get; set;
        }

        public virtual Publication? Publication
        {
            get; set;
        }
    }
}
