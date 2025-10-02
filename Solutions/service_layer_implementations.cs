// =====================================================
// 1. EXTENDED INTERFACES
// =====================================================

// File: IPublisherService.cs (NEW FILE)
using LTF_Library_V1.DTOs;

namespace LTF_Library_V1.Services
{
    public interface IPublisherService
    {
        Task<List<PublisherDto>> GetPublishersAsync();
        Task<PublisherDto?> GetPublisherAsync(int publisherId);
        Task<ServiceResult> AddPublisherAsync(PublisherDto publisher);
        Task<ServiceResult> UpdatePublisherAsync(PublisherDto publisher);
        Task<ServiceResult> DeletePublisherAsync(int publisherId);
    }
}

// File: ICreatorService.cs (NEW FILE)
using LTF_Library_V1.DTOs;

namespace LTF_Library_V1.Services
{
    public interface ICreatorService
    {
        Task<List<CreatorDto>> GetCreatorsAsync();
        Task<CreatorDto?> GetCreatorAsync(int creatorId);
        Task<ServiceResult> AddCreatorAsync(CreatorDto creator);
        Task<ServiceResult> UpdateCreatorAsync(CreatorDto creator);
        Task<ServiceResult> DeleteCreatorAsync(int creatorId);
    }
}

// File: IGenreService.cs (NEW FILE)
using LTF_Library_V1.DTOs;

namespace LTF_Library_V1.Services
{
    public interface IGenreService
    {
        Task<List<GenreDto>> GetGenresAsync();
        Task<GenreDto?> GetGenreAsync(int genreId);
        Task<ServiceResult> AddGenreAsync(GenreDto genre);
        Task<ServiceResult> UpdateGenreAsync(GenreDto genre);
        Task<ServiceResult> DeleteGenreAsync(int genreId);
    }
}

// =====================================================
// 2. EXTENDED DTOs (Add to existing PublicationDtos.cs)
// =====================================================

namespace LTF_Library_V1.DTOs
{
    // Add this to your existing PublicationDtos.cs file
    
    public class PublicationEditDto
    {
        public int PublicationID { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string PublicationTitle { get; set; } = string.Empty;
        
        public string? CatalogNumber { get; set; }
        public string? Comments { get; set; }
        public string? CoverPhotoLink { get; set; }
        public string? Edition { get; set; }
        public string? ISBN { get; set; }
        public int? Volume { get; set; }
        public int? NumberOfVolumes { get; set; }
        public int? Pages { get; set; }
        public string? YearPublished { get; set; }
        public int? ConfidenceLevel { get; set; }
        public decimal? ListPrice { get; set; }
        
        // Foreign Keys
        public int? PublisherID { get; set; }
        public int? MediaTypeID { get; set; }
        public int? MediaConditionID { get; set; }
        public int? ShelfID { get; set; }
        
        // Related Collections
        public List<CreatorDto> SelectedAuthors { get; set; } = new();
        public List<GenreDto> SelectedCategories { get; set; } = new();
        public List<string> Keywords { get; set; } = new();
    }

    public class PublisherDto
    {
        public int PublisherID { get; set; }
        public string Publisher1 { get; set; } = string.Empty;
        public string? PublisherGoogle { get; set; }
        
        public string DisplayName => 
            !string.IsNullOrEmpty(PublisherGoogle) && Publisher1 != PublisherGoogle
                ? $"{Publisher1} ({PublisherGoogle})"
                : Publisher1;
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        
        public static ServiceResult Successful(string message = "Operation completed successfully", object? data = null)
        {
            return new ServiceResult { Success = true, Message = message, Data = data };
        }
        
        public static ServiceResult Failed(string message)
        {
            return new ServiceResult { Success = false, Message = message };
        }
    }
}

// =====================================================
// 3. PUBLISHER SERVICE IMPLEMENTATION
// =====================================================

// File: PublisherService.cs (NEW FILE)
using LTF_Library_V1.DTOs;
using LTF_Library_V1.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LTF_Library_V1.Services
{
    public class PublisherService : IPublisherService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PublisherService> _logger;

        public PublisherService(ApplicationDbContext context, ILogger<PublisherService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PublisherDto>> GetPublishersAsync()
        {
            try
            {
                return await _context.Publishers
                    .OrderBy(p => p.Publisher1)
                    .Select(p => new PublisherDto
                    {
                        PublisherID = p.PublisherID,
                        Publisher1 = p.Publisher1 ?? "",
                        PublisherGoogle = p.PublisherGoogle
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving publishers");
                throw;
            }
        }

        public async Task<PublisherDto?> GetPublisherAsync(int publisherId)
        {
            try
            {
                var publisher = await _context.Publishers
                    .FirstOrDefaultAsync(p => p.PublisherID == publisherId);

                if (publisher == null) return null;

                return new PublisherDto
                {
                    PublisherID = publisher.PublisherID,
                    Publisher1 = publisher.Publisher1 ?? "",
                    PublisherGoogle = publisher.PublisherGoogle
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving publisher {PublisherId}", publisherId);
                throw;
            }
        }

        public async Task<ServiceResult> AddPublisherAsync(PublisherDto publisherDto)
        {
            try
            {
                // Check for duplicates
                var existing = await _context.Publishers
                    .FirstOrDefaultAsync(p => p.Publisher1 == publisherDto.Publisher1);

                if (existing != null)
                {
                    return ServiceResult.Failed("A publisher with this name already exists.");
                }

                var publisher = new Publisher
                {
                    Publisher1 = publisherDto.Publisher1,
                    PublisherGoogle = publisherDto.PublisherGoogle
                };

                _context.Publishers.Add(publisher);
                await _context.SaveChangesAsync();

                publisherDto.PublisherID = publisher.PublisherID;
                return ServiceResult.Successful("Publisher added successfully.", publisherDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding publisher");
                return ServiceResult.Failed("An error occurred while adding the publisher.");
            }
        }

        public async Task<ServiceResult> UpdatePublisherAsync(PublisherDto publisherDto)
        {
            try
            {
                var publisher = await _context.Publishers
                    .FirstOrDefaultAsync(p => p.PublisherID == publisherDto.PublisherID);

                if (publisher == null)
                {
                    return ServiceResult.Failed("Publisher not found.");
                }

                // Check for duplicates (excluding current record)
                var existing = await _context.Publishers
                    .FirstOrDefaultAsync(p => p.Publisher1 == publisherDto.Publisher1 && 
                                            p.PublisherID != publisherDto.PublisherID);

                if (existing != null)
                {
                    return ServiceResult.Failed("A publisher with this name already exists.");
                }

                publisher.Publisher1 = publisherDto.Publisher1;
                publisher.PublisherGoogle = publisherDto.PublisherGoogle;

                await _context.SaveChangesAsync();
                return ServiceResult.Successful("Publisher updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating publisher {PublisherId}", publisherDto.PublisherID);
                return ServiceResult.Failed("An error occurred while updating the publisher.");
            }
        }

        public async Task<ServiceResult> DeletePublisherAsync(int publisherId)
        {
            try
            {
                var publisher = await _context.Publishers
                    .FirstOrDefaultAsync(p => p.PublisherID == publisherId);

                if (publisher == null)
                {
                    return ServiceResult.Failed("Publisher not found.");
                }

                // Check if publisher is being used
                var isUsed = await _context.Publications
                    .AnyAsync(p => p.PublisherID == publisherId);

                if (isUsed)
                {
                    return ServiceResult.Failed("Cannot delete publisher because it is referenced by one or more publications.");
                }

                _context.Publishers.Remove(publisher);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("Publisher deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting publisher {PublisherId}", publisherId);
                return ServiceResult.Failed("An error occurred while deleting the publisher.");
            }
        }
    }
}

// =====================================================
// 4. EXTENDED PUBLICATION SERVICE
// =====================================================

// Add these methods to your existing PublicationService.cs

public partial class PublicationService // Make your existing class partial
{
    // Add these new methods to your existing PublicationService.cs

    public async Task<PublicationEditDto?> GetPublicationForEditAsync(int publicationId)
    {
        try
        {
            var publication = await _context.Publications
                .Include(p => p.Publisher)
                .Include(p => p.MediaType)
                .Include(p => p.MediaCondition)
                .Include(p => p.Shelf)
                    .ThenInclude(s => s!.Bookcase)
                .Include(p => p.PublicationCreators)
                    .ThenInclude(pc => pc.Creator)
                .Include(p => p.PublicationGenres)
                    .ThenInclude(pg => pg.Genre)
                .Include(p => p.PublicationKeyWords)
                .FirstOrDefaultAsync(p => p.PublicationID == publicationId);

            if (publication == null) return null;

            return new PublicationEditDto
            {
                PublicationID = publication.PublicationID,
                PublicationTitle = publication.PublicationTitle,
                CatalogNumber = publication.CatalogNumber,
                Comments = publication.Comments,
                CoverPhotoLink = publication.CoverPhotoLink,
                Edition = publication.Edition,
                ISBN = publication.ISBN,
                Volume = publication.Volume,
                NumberOfVolumes = publication.NumberOfVolumes,
                Pages = publication.Pages,
                YearPublished = publication.YearPublished,
                ConfidenceLevel = publication.ConfidenceLevel,
                ListPrice = publication.ListPrice,
                PublisherID = publication.PublisherID,
                MediaTypeID = publication.MediaTypeID,
                MediaConditionID = publication.MediaConditionID,
                ShelfID = publication.ShelfID,
                SelectedAuthors = publication.PublicationCreators
                    .Where(pc => pc.Creator != null)
                    .Select(pc => new CreatorDto
                    {
                        CreatorID = pc.Creator!.CreatorID,
                        CreatorFirstName = pc.Creator.CreatorFirstName,
                        CreatorMiddleName = pc.Creator.CreatorMiddleName,
                        CreatorLastName = pc.Creator.CreatorLastName,
                        FullName = pc.Creator.FullName,
                        SortName = pc.Creator.SortName
                    }).ToList(),
                SelectedCategories = publication.PublicationGenres
                    .Where(pg => pg.Genre != null)
                    .Select(pg => new GenreDto
                    {
                        GenreID = pg.Genre!.GenreID,
                        Genre = pg.Genre.Genre1 ?? "",
                        SortOrder = pg.Genre.SortOrder
                    }).ToList(),
                Keywords = publication.PublicationKeyWords
                    .Where(pk => !string.IsNullOrEmpty(pk.KeyWord))
                    .Select(pk => pk.KeyWord!)
                    .ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving publication for edit {PublicationId}", publicationId);
            throw;
        }
    }

    public async Task<ServiceResult> UpdatePublicationAsync(PublicationEditDto publicationDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var publication = await _context.Publications
                .Include(p => p.PublicationCreators)
                .Include(p => p.PublicationGenres)
                .Include(p => p.PublicationKeyWords)
                .FirstOrDefaultAsync(p => p.PublicationID == publicationDto.PublicationID);

            if (publication == null)
            {
                return ServiceResult.Failed("Publication not found.");
            }

            // Update basic publication properties
            publication.PublicationTitle = publicationDto.PublicationTitle;
            publication.CatalogNumber = publicationDto.CatalogNumber;
            publication.Comments = publicationDto.Comments;
            publication.CoverPhotoLink = publicationDto.CoverPhotoLink;
            publication.Edition = publicationDto.Edition;
            publication.ISBN = publicationDto.ISBN;
            publication.Volume = publicationDto.Volume;
            publication.NumberOfVolumes = publicationDto.NumberOfVolumes;
            publication.Pages = publicationDto.Pages;
            publication.YearPublished = publicationDto.YearPublished;
            publication.ConfidenceLevel = publicationDto.ConfidenceLevel;
            publication.ListPrice = publicationDto.ListPrice;
            publication.PublisherID = publicationDto.PublisherID;
            publication.MediaTypeID = publicationDto.MediaTypeID;
            publication.MediaConditionID = publicationDto.MediaConditionID;
            publication.ShelfID = publicationDto.ShelfID;

            // Update Authors (using Entity Framework approach)
            // Remove existing author relationships
            _context.PublicationCreators.RemoveRange(publication.PublicationCreators);

            // Add new author relationships
            foreach (var author in publicationDto.SelectedAuthors)
            {
                publication.PublicationCreators.Add(new PublicationCreator
                {
                    PublicationID = publication.PublicationID,
                    CreatorID = author.CreatorID
                });
            }

            // Update Categories (using Entity Framework approach)
            // Remove existing category relationships
            _context.PublicationGenres.RemoveRange(publication.PublicationGenres);

            // Add new category relationships
            foreach (var category in publicationDto.SelectedCategories)
            {
                publication.PublicationGenres.Add(new PublicationGenre
                {
                    PublicationID = publication.PublicationID,
                    GenreID = category.GenreID
                });
            }

            // Update Keywords (using Entity Framework approach)
            // Remove existing keywords
            _context.PublicationKeyWords.RemoveRange(publication.PublicationKeyWords);

            // Add new keywords
            foreach (var keyword in publicationDto.Keywords.Where(k => !string.IsNullOrWhiteSpace(k)))
            {
                publication.PublicationKeyWords.Add(new PublicationKeyWord
                {
                    PublicationID = publication.PublicationID,
                    KeyWord = keyword.Trim()
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult.Successful("Publication updated successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating publication {PublicationId}", publicationDto.PublicationID);
            return ServiceResult.Failed("An error occurred while updating the publication.");
        }
    }

    public async Task<List<string>> GetAllKeywordsAsync()
    {
        try
        {
            return await _context.PublicationKeyWords
                .Where(pk => !string.IsNullOrEmpty(pk.KeyWord))
                .Select(pk => pk.KeyWord!)
                .Distinct()
                .OrderBy(k => k)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving keywords");
            throw;
        }
    }

    public async Task<List<string>> GetKeywordsForPublicationAsync(int publicationId)
    {
        try
        {
            return await _context.PublicationKeyWords
                .Where(pk => pk.PublicationID == publicationId && !string.IsNullOrEmpty(pk.KeyWord))
                .Select(pk => pk.KeyWord!)
                .OrderBy(k => k)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving keywords for publication {PublicationId}", publicationId);
            throw;
        }
    }
}

// =====================================================
// 5. STORED PROCEDURE VERSION (Alternative Implementation)
// =====================================================

// Alternative UpdatePublicationAsync using stored procedures
public async Task<ServiceResult> UpdatePublicationUsingStoredProceduresAsync(PublicationEditDto publicationDto)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // Update basic publication info using EF
        var publication = await _context.Publications
            .FirstOrDefaultAsync(p => p.PublicationID == publicationDto.PublicationID);

        if (publication == null)
        {
            return ServiceResult.Failed("Publication not found.");
        }

        // Update basic properties
        publication.PublicationTitle = publicationDto.PublicationTitle;
        publication.CatalogNumber = publicationDto.CatalogNumber;
        publication.Comments = publicationDto.Comments;
        publication.CoverPhotoLink = publicationDto.CoverPhotoLink;
        publication.Edition = publicationDto.Edition;
        publication.ISBN = publicationDto.ISBN;
        publication.Volume = publicationDto.Volume;
        publication.NumberOfVolumes = publicationDto.NumberOfVolumes;
        publication.Pages = publicationDto.Pages;
        publication.YearPublished = publicationDto.YearPublished;
        publication.ConfidenceLevel = publicationDto.ConfidenceLevel;
        publication.ListPrice = publicationDto.ListPrice;
        publication.PublisherID = publicationDto.PublisherID;
        publication.MediaTypeID = publicationDto.MediaTypeID;
        publication.MediaConditionID = publicationDto.MediaConditionID;
        publication.ShelfID = publicationDto.ShelfID;

        await _context.SaveChangesAsync();

        // Update relationships using stored procedures
        var authorIds = string.Join(",", publicationDto.SelectedAuthors.Select(a => a.CreatorID));
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_UpdatePublicationAuthors @PublicationID = {0}, @AuthorIDs = {1}",
            publicationDto.PublicationID, authorIds);

        var genreIds = string.Join(",", publicationDto.SelectedCategories.Select(c => c.GenreID));
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_UpdatePublicationGenres @PublicationID = {0}, @GenreIDs = {1}",
            publicationDto.PublicationID, genreIds);

        var keywordsJson = System.Text.Json.JsonSerializer.Serialize(publicationDto.Keywords);
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_UpdatePublicationKeywords @PublicationID = {0}, @Keywords = {1}",
            publicationDto.PublicationID, keywordsJson);

        await transaction.CommitAsync();
        return ServiceResult.Successful("Publication updated successfully.");
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error updating publication {PublicationId}", publicationDto.PublicationID);
        return ServiceResult.Failed("An error occurred while updating the publication.");
    }
}

// =====================================================
// 6. DEPENDENCY INJECTION REGISTRATION
// =====================================================

// Add this to your Program.cs in the service registration section:

// Register new services
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<ICreatorService, CreatorService>();
builder.Services.AddScoped<IGenreService, GenreService>();

// Note: You'll need to implement CreatorService and GenreService similarly to PublisherService