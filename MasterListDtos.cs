using System.ComponentModel.DataAnnotations;

namespace LTF_Library_V1.DTOs
{
    // Generic DTO for simple master list operations
    public class MasterListItemDto
    {
        public int Id
        {
            get; set;
        }
        public string Name { get; set; } = string.Empty;
        public int ? RelatedId
        {
            get; set;
        }
        public int? SortOrder
        {
            get; set;
        }
        public string? AdditionalField1
        {
            get; set;
        }
        public string? AdditionalField2
        {
            get; set;
        }
        public string? AdditionalField3
        {
            get; set;
        }
    }

    // =============================================
    // BOOKCASE
    // =============================================
    public class BookcaseDto
    {
        public int BookcaseID
        {
            get; set;
        }

        [Required(ErrorMessage = "Bookcase name is required")]
        [StringLength(50, ErrorMessage = "Bookcase name cannot exceed 50 characters")]
        public string Bookcase1 { get; set; } = string.Empty;
        public string ? BookcaseDescription
        {
            get; set;
        }
    }

    // =============================================
    // MEDIA CONDITION
    // =============================================
    public class MediaConditionDto
    {
        public int MediaConditionID
        {
            get; set;
        }

        [Required(ErrorMessage = "Media condition is required")]
        [StringLength(50, ErrorMessage = "Media condition cannot exceed 50 characters")]
        public string MediaCondition { get; set; } = string.Empty;

        public int? SortOrder
        {
            get; set;
        }
    }

    // =============================================
    // PARTICIPANT
    // =============================================
    public class ParticipantDto
    {
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
       
        public string FullName { get; set; } = string.Empty;
        public string SortName { get; set; } = string.Empty;
    }

    // =============================================
    // PARTICIPANT STATUS
    // =============================================
    public class ParticipantStatusDto
    {
        public int ParticipantStatusID
        {
            get; set;
        }

        [Required(ErrorMessage = "Participant status is required")]
        [StringLength(255, ErrorMessage = "Participant status cannot exceed 255 characters")]
        public string ParticipantStatus1 { get; set; } = string.Empty;
        // Extended description added 2025-12-22
        [Required(ErrorMessage = "Extended description is required")]
        [StringLength(100, ErrorMessage = "Extended description cannot exceed 100 characters")]
        public string ExtendedDescription{
            get; set;
        } = string.Empty;
        [Required(ErrorMessage = "Transaction Type is required")]
        [StringLength(10, ErrorMessage = "Transaction Type cannot exceed 10 characters")]
        public string TransactionType
        {
            get; set;
        } = string.Empty;

        public int? SortOrder
        {
            get; set;
        }
    }

    // =============================================
    // SHELF
    // =============================================
    public class ShelfDto
    {
        public int ShelfID
        {
            get; set;
        }

        [Required(ErrorMessage = "Shelf name is required")]
        [StringLength(50, ErrorMessage = "Shelf name cannot exceed 50 characters")]
        public string Shelf1 { get; set; } = string.Empty;

        public int? BookcaseID
        {
            get; set;
        }
        public string? Bookcase1
        {
            get; set;
        }
        public string? ShelfDescription
        {
            get; set;
        }
    }

    // =============================================
    // OPERATION RESULT (for service responses)
    // =============================================
    public class OperationResultDto
    {
        public bool Success
        {
            get; set;
        }
        public string Message { get; set; } = string.Empty;
        public int? AffectedId
        {
            get; set;
        }
    }

    // NOTE: GenreDto, MediaTypeDto, CreatorDto, and PublisherDto are already 
    // defined in PublicationDtos.cs, so they are not duplicated here.
}