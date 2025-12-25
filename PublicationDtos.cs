using System.ComponentModel.DataAnnotations;

namespace LTF_Library_V1.DTOs
{
    public class PublicationSearchCriteria
    {
        public string? Title
        {
            get; set;
        }
        public string? CreatorId
        {
            get; set;
        }
        public string? GenreId
        {
            get; set;
        }
        public string? MediaTypeId
        {
            get; set;
        }
        public string? Keyword
        {
            get; set;
        }
        public string? YearFrom
        {
            get; set;
        }
        public string? YearTo
        {
            get; set;
        }
    }

    public class PublicationSearchRequest
    {
        public PublicationSearchCriteria Criteria { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "title";
        public bool SortDescending { get; set; } = false;
    }

    public class PublicationSearchResult
    {
        public int PublicationID
        {
            get; set;
        }
        public string PublicationTitle { get; set; } = string.Empty;
        public List<string> Authors { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public string? YearPublished
        {
            get; set;
        }
        public string MediaTypeName { get; set; } = string.Empty;
        public string? Comments
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
        public string? PublisherName
        {
            get; set;
        }
        public string? CoverPhotoLink
        {
            get; set;
        }
        public string? Pages
        {
            get; set;
        }
    }

    public class PublicationSearchResponse
    {
        public List<PublicationSearchResult> Results { get; set; } = new();
        public int TotalCount
        {
            get; set;
        }
        public int Page
        {
            get; set;
        }
        public int PageSize
        {
            get; set;
        }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class PublicationDetailDto
    {
        public int PublicationID
        {
            get; set;
        }
        public string PublicationTitle { get; set; } = string.Empty;
        public string? CatalogNumber
        {
            get; set;
        }
        public string? Comments
        {
            get; set;
        }
        public string? CoverPhotoLink
        {
            get; set;
        }
        public string? Edition
        {
            get; set;
        }
        public string? ISBN
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
        public string? Printing
        {
            get; set;
        }
        public string? YearPublished
        {
            get; set;
        }
        public int? ConfidenceLevel
        {
            get; set;
        }
        public string? InternalComments
        {
            get; set;
        }   
        public DateTime? DateCaptured
        {
            get; set;
        }
        public decimal? ListPrice
        {
            get; set;
        }

        // Related data
        public List<string> Authors { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<string> Keywords { get; set; } = new();
        public string? PublisherName
        {
            get; set;
        }
        public string MediaTypeName { get; set; } = string.Empty;
        public string? MediaConditionName
        {
            get; set;
        }
        public string? ShelfLocation
        {
            get; set;
        }
    }

    public class CreatorDto
    {
        public int CreatorID
        {
            get; set;
        }
        public string? CreatorFirstName
        {
            get; set;
        }
        public string? CreatorMiddleName
        {
            get; set;
        }
        public string? CreatorLastName
        {
            get; set;
        }
        public string FullName { get; set; } = string.Empty;
        public string SortName { get; set; } = string.Empty;
    }

    public class GenreDto
    {
        public int GenreID
        {
            get; set;
        }
        public string Genre { get; set; } = string.Empty;
        public int? SortOrder
        {
            get; set;
        }
    }

    public class MediaTypeDto
    {
        public int MediaTypeID
        {
            get; set;
        }
        public string MediaType { get; set; } = string.Empty;
        public int? SortOrder
        {
            get; set;
        }
    }

    public class CollectionStatisticsDto
    {
        public int TotalBooks
        {
            get; set;
        }
        public int TotalPeriodicals
        {
            get; set;
        }
        public int TotalNewspapers
        {
            get; set;
        }
        public int TotalManuscripts
        {
            get; set;
        }
        public int TotalAudioRecordings
        {
            get; set;
        }
        public int TotalDocuments
        {
            get; set;
        }
        public int TotalItems
        {
            get; set;
        }
        public string EarliestYear { get; set; } = string.Empty;
        public string LatestYear { get; set; } = string.Empty;
    }

    public enum RequestType
    {
        Borrow,
        Abstract
    }

    public class PublicationRequestDto
    {
        public int PublicationId
        {
            get; set;
        }

        [Required]
        public string RequestType
        {
            get; set;
        } = "Borrow";

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

        [Phone]
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
    }

    public class PublicationRequestSubmissionDto
    {
        public bool Success
        {
            get; set;
        }
        public string Message { get; set; } = string.Empty;
        public int? RequestId
        {
            get; set;
        }
    }

    public class PublicationEditDto
    {
        public int PublicationID
        {
            get; set;
        }
        public string PublicationTitle { get; set; } = string.Empty;
        public string? CatalogNumber
        {
            get; set;
        }
        public string? Comments
        {
            get; set;
        }
        public string? CoverPhotoLink
        {
            get; set;
        }
        public string? Edition
        {
            get; set;
        }
        public string? ISBN
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
        public string? Printing
        {
            get; set;
        }
        public string? YearPublished
        {
            get; set;
        }
        public int? ConfidenceLevel
        {
            get; set;
        }
        public string? InternalComments
        {
            get; set;
        }
        public decimal? ListPrice
        {
            get; set;
        }

        // Foreign Keys
        public int? PublisherID
        {
            get; set;
        }
        public int? MediaTypeID
        {
            get; set;
        }
        public int? MediaConditionID
        {
            get; set;
        }
        public int? ShelfID
        {
            get; set;
        }

        // Related collections (IDs)
        public List<int> AuthorIds { get; set; } = new();
        public List<int> GenreIds { get; set; } = new();
        public List<string> Keywords { get; set; } = new();

        // UI Binding properties (used by Admin.razor) - full objects for display
        public List<CreatorDto> SelectedAuthors { get; set; } = new();
        public List<GenreDto> SelectedCategories { get; set; } = new();
    }

    public class PublisherDto
    {
        public int PublisherID
        {
            get; set;
        }
        public string Publisher { get; set; } = string.Empty;
        public string? PublisherGoogle
        {
            get; set;
        }
 
    }

 
}