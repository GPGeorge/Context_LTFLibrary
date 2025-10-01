using LTF_Library_V1.Data;
using LTF_Library_V1.DTOs;
using LTF_Library_V1.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


namespace LTF_Library_V1.Services
{
    public class PublicationService(ApplicationDbContext context, ILogger<PublicationService> logger) : IPublicationService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<PublicationService> _logger = logger;

        public async Task<PublicationSearchResponse> SearchPublicationsAsync(PublicationSearchRequest request)
        {
            //console.writeLine($"[{DateTime.Now:HH:mm:ss}] Search started");
            try
            {
                var query = _context.Publications
                    .Include(p => p.PublicationCreators)
                        .ThenInclude(pc => pc.Creator)
                    .Include(p => p.PublicationGenres)
                        .ThenInclude(pg => pg.Genre)
                    .Include(p => p.MediaType)
                    .Include(p => p.Publisher)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(request.Criteria.Title))
                {
                    query = query.Where(p => p.PublicationTitle.Contains(request.Criteria.Title));
                }

                if (!string.IsNullOrWhiteSpace(request.Criteria.CreatorId) && int.TryParse(request.Criteria.CreatorId, out int creatorId))
                {
                    query = query.Where(p => p.PublicationCreators.Any(pc => pc.CreatorID == creatorId));
                }

                if (!string.IsNullOrWhiteSpace(request.Criteria.GenreId) && int.TryParse(request.Criteria.GenreId, out int genreId))
                {
                    query = query.Where(p => p.PublicationGenres.Any(pg => pg.GenreID == genreId));
                }

                if (!string.IsNullOrWhiteSpace(request.Criteria.MediaTypeId) && int.TryParse(request.Criteria.MediaTypeId, out int mediaTypeId))
                {
                    query = query.Where(p => p.MediaTypeID == mediaTypeId);
                }

                // Apply sorting
                query = request.SortBy.ToLower() switch
                {
                    "author" => query.OrderBy(p => p.PublicationCreators.OrderBy(pc => pc.Creator!.CreatorLastName).First().Creator!.CreatorLastName),
                    "year" => query.OrderBy(p => p.YearPublished),
                    "type" => query.OrderBy(p => p.MediaType!.MediaType1),
                    _ => query.OrderBy(p => p.PublicationTitle)
                };

                // Get total count
                var totalCount = await query.CountAsync();

                _logger.LogInformation($"Total query results: {totalCount}");
                _logger.LogInformation($"Requesting page: {request.Page}, PageSize: {request.PageSize}");
                _logger.LogInformation($"Skip: {( request.Page - 1 ) * request.PageSize}, Take: {request.PageSize}");

                // Apply pagination
                var results = await query
                    .Skip(( request.Page - 1 ) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new PublicationSearchResult
                    {
                        PublicationID = p.PublicationID,
                        PublicationTitle = p.PublicationTitle,
                        Authors = p.PublicationCreators
                            .Select(pc => pc.Creator!.FullName)
                            .ToList(),
                        Categories = p.PublicationGenres
                            .Select(pg => pg.Genre!.Genre1!)
                            .ToList(),
                        YearPublished = p.YearPublished,
                        MediaTypeName = p.MediaType!.MediaType1 ?? "",
                        Comments = p.Comments,
                        Volume = p.Volume,
                        NumberOfVolumes = p.NumberOfVolumes,
                        PublisherName = p.Publisher!.Publisher1,
                        CoverPhotoLink = p.CoverPhotoLink,
                        Pages = p.Pages.ToString()
                        
                        
                    })
                    .ToListAsync();

                _logger.LogInformation($"Returned {results.Count} results");

                //console.writeLine($"[{DateTime.Now:HH:mm:ss}] Search completed successfully");

                return new PublicationSearchResponse
                {
                    Results = results,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                //console.writeLine($"[{DateTime.Now:HH:mm:ss}] Search failed: {ex.Message}");
                _logger.LogError(ex, "Error searching publications");
                throw;
            }
        }

        public async Task<PublicationDetailDto?> GetPublicationDetailAsync(int publicationId)
        {
            try
            {
                var publication = await _context.Publications
                    .AsNoTracking() // Add this line
                    .Include(p => p.PublicationCreators)
                        .ThenInclude(pc => pc.Creator)
                    .Include(p => p.PublicationGenres)
                        .ThenInclude(pg => pg.Genre)
                    .Include(p => p.PublicationKeyWords)
                    .Include(p => p.MediaType)
                    .Include(p => p.MediaCondition)
                    .Include(p => p.Publisher)
                    .Include(p => p.Shelf)
                        .ThenInclude(s => s!.Bookcase)
                    .FirstOrDefaultAsync(p => p.PublicationID == publicationId);

                if (publication == null)
                    return null;

                return new PublicationDetailDto
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
                    Printing = publication.Printing,
                    YearPublished = publication.YearPublished,
                    ConfidenceLevel = publication.ConfidenceLevel,
                    DateCaptured = publication.DateCaptured,
                    ListPrice = publication.ListPrice,
                    Authors = publication.PublicationCreators
                        .Select(pc => pc.Creator!.FullName)
                        .ToList(),
                    Categories = publication.PublicationGenres
                        .Select(pg => pg.Genre!.Genre1!)
                        .ToList(),
                    Keywords = publication.PublicationKeyWords
                        .Select(pkw => pkw.KeyWord!)
                        .ToList(),
                    PublisherName = publication.Publisher?.DisplayName,
                    MediaTypeName = publication.MediaType?.MediaType1 ?? "",
                    MediaConditionName = publication.MediaCondition?.MediaCondition1,
                    ShelfLocation = publication.Shelf?.Location
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting publication detail for ID {PublicationId}", publicationId);
                throw;
            }
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
                        Publisher1 = p.Publisher1,
                        PublisherGoogle = p.PublisherGoogle
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting publishers");
                throw;
            }
        }
      
        public async Task<List<CreatorDto>> GetAuthorsAsync()
        {
            try
            {
                _logger.LogInformation("Starting GetAuthorsAsync...");
                var count = await _context.Creators.CountAsync();
                _logger.LogInformation($"Total creators in database: {count}");

                var authors = await _context.Creators
                    .OrderBy(c=> c.CreatorLastName)
                    .ThenBy(c=> c.CreatorFirstName) 
                    .Select(c => new CreatorDto
                    {
                        CreatorID = c.CreatorID,
                        CreatorFirstName = c.CreatorFirstName,
                        CreatorMiddleName = c.CreatorMiddleName,
                        CreatorLastName = c.CreatorLastName,
                        FullName = $"{c.CreatorFirstName ?? "Unknown"} {c.CreatorLastName ?? "Unknown"}".Trim(),
                        SortName = $"{c.CreatorLastName ?? "Unknown"} {c.CreatorFirstName ?? "Unknown"}".Trim()
                    })
                    .ToListAsync();

                _logger.LogInformation($"GetAuthorsAsync completed - returned {authors.Count} authors");
                return authors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAuthorsAsync");
                throw;
            }
        }

        public async Task<List<GenreDto>> GetGenresAsync()
        {
            try
            {
                _logger.LogInformation("Starting GetGenresAsync...");

                // Simplify the query first to test
                var count = await _context.Genres.CountAsync();
                _logger.LogInformation($"Total genres in database: {count}");

                var genres = await _context.Genres
                    .OrderBy(g => g.Genre1)
                    .Select(g => new GenreDto
                    {
                        GenreID = g.GenreID,
                        Genre = g.Genre1 ?? "",
                        SortOrder = g.SortOrder
                    })
                    .ToListAsync();

                _logger.LogInformation($"GetGenresAsync completed - returned {genres.Count} genres");
                return genres;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetGenresAsync");
                throw;
            }
        }

        public async Task<List<MediaTypeDto>> GetMediaTypesAsync()
        {
            try
            {
                return await _context.MediaTypes
                    .Where(mt => mt.Publications.Any()) // Only types with publications
                    .OrderBy(mt => mt.SortOrder)
                    .ThenBy(mt => mt.MediaType1)
                    .Select(mt => new MediaTypeDto
                    {
                        MediaTypeID = mt.MediaTypeID,
                        MediaType = mt.MediaType1!,
                        SortOrder = mt.SortOrder
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting media types");
                throw;
            }
        }

        public async Task<CollectionStatisticsDto> GetCollectionStatisticsAsync()
        {
            try
            {
                var stats = await _context.Publications
                    .Include(p => p.MediaType)
                    .GroupBy(p => p.MediaType!.MediaType1)
                    .Select(g => new { MediaType = g.Key, Count = g.Count() })
                    .ToListAsync();

                var audioCount = stats.Where(s => s.MediaType == "DVD/CD" ||
                                                  s.MediaType == "VHS Tape" ||
                                                  s.MediaType == "Vinyl Record").Sum(s => s.Count);
                    

                var totalItems = await _context.Publications.CountAsync();
                var years = await _context.Publications
                    .Where(p => !string.IsNullOrEmpty(p.YearPublished))
                    .Select(p => p.YearPublished!)
                    .ToListAsync();

                return new CollectionStatisticsDto
                {
                    TotalBooks = stats.FirstOrDefault(s => s.MediaType == "Book")?.Count ?? 0,
                    TotalPeriodicals = stats.FirstOrDefault(s => s.MediaType == "Periodical")?.Count ?? 0,
                    TotalNewspapers = stats.FirstOrDefault(s => s.MediaType == "Newspaper")?.Count ?? 0,
                    TotalManuscripts = stats.FirstOrDefault(s => s.MediaType == "Manuscript")?.Count ?? 0,
                    TotalAudioRecordings = audioCount,
                    TotalDocuments = stats.FirstOrDefault(s => s.MediaType == "Document")?.Count ?? 0,
                    TotalItems = totalItems,
                    EarliestYear = years.Count != 0 ? years.Min() : "",
                    LatestYear = years.Count != 0 ? years.Max() : ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collection statistics");
                throw;
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
        public async Task<PublicationEditDto?> GetPublicationForEditAsync(int publicationId)
        {
            try
            {
                var publication = await _context.Publications
                    .AsNoTracking() // Ensure fresh data
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

                if (publication == null)
                    return null;

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
        //Here
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

                // Update basic publication properties with scalar values
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

                // Update Authors - Remove existing and add new using stored procedure with a transaction

                var authorIds = string.Join(",", publicationDto.SelectedAuthors.Select(a => a.CreatorID));

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_UpdatePublicationAuthors @PublicationID, @AuthorIDs",
                    new SqlParameter("@PublicationID", publication.PublicationID),
                    new SqlParameter("@AuthorIDs", authorIds));

                // Update Categories - Remove existing and add new using stored procedure with a transaction

                var categoryIDs = string.Join(",", publicationDto.SelectedCategories.Select(a => a.GenreID));

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_UpdatePublicationCategories @PublicationID, @categoryIDs",
                    new SqlParameter("@PublicationID", publication.PublicationID),
                    new SqlParameter("@categoryIDs", categoryIDs));

                // Update Keywords - Remove existing and add new using stored procedure with a transaction

                var keywordJson = JsonSerializer.Serialize(publicationDto.Keywords.Where(k => !string.IsNullOrWhiteSpace(k)));

                await _context.Database.ExecuteSqlRawAsync("EXEC sp_UpdatePublicationKeywords @PublicationID, @Keywords",
                      new SqlParameter("@PublicationID", publication.PublicationID),
                      new SqlParameter("@Keywords", keywordJson));

                 

                await transaction.CommitAsync();

                _context.ChangeTracker.Clear();

                // Fetch fresh data from database
                var refreshedPublication = await GetPublicationForEditAsync(publicationDto.PublicationID);

                if (refreshedPublication != null)
                {
                    return ServiceResult.Successful("Publication updated successfully.", refreshedPublication);
                }
                else
                {
                    // Still successful but couldn't get refreshed data
                    return ServiceResult.Successful("Publication updated successfully.");
                }
                            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating publication {PublicationId}", publicationDto.PublicationID);
                return ServiceResult.Failed("An error occurred while updating the publication.");
            }
        }
        //to here
        //Here
        public async Task<ServiceResult> CreatePublicationAsync(PublicationEditDto publicationDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var publication = new Publication
                {
                
                // Insert basic publication properties with scalar values
                PublicationTitle = publicationDto.PublicationTitle,
                CatalogNumber = publicationDto.CatalogNumber,
                Comments = publicationDto.Comments,
                CoverPhotoLink = publicationDto.CoverPhotoLink,
                Edition = publicationDto.Edition,
                ISBN = publicationDto.ISBN,
                Volume = publicationDto.Volume,
                NumberOfVolumes = publicationDto.NumberOfVolumes,
                Pages = publicationDto.Pages,
                YearPublished = publicationDto.YearPublished,
                ConfidenceLevel = publicationDto.ConfidenceLevel,
                ListPrice = publicationDto.ListPrice,
                PublisherID = publicationDto.PublisherID,
                MediaTypeID = publicationDto.MediaTypeID,
                MediaConditionID = publicationDto.MediaConditionID,
                ShelfID = publicationDto.ShelfID};

                _context.Publications.Add(publication);

                await _context.SaveChangesAsync();

                // Add Authors using stored procedure
                if (publicationDto.SelectedAuthors?.Any() == true)
                {
                    var authorIds = string.Join(",", publicationDto.SelectedAuthors.Select(a => a.CreatorID));
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC sp_UpdatePublicationAuthors @PublicationID, @AuthorIDs",
                        new SqlParameter("@PublicationID", publication.PublicationID),
                        new SqlParameter("@AuthorIDs", authorIds));
                }
                // Add Categories using stored procedure
                if (publicationDto.SelectedCategories?.Any() == true)
                {
                    var categoryIDs = string.Join(",", publicationDto.SelectedCategories.Select(c => c.GenreID));
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC sp_UpdatePublicationCategories @PublicationID, @CategoryIDs",
                        new SqlParameter("@PublicationID", publication.PublicationID),
                        new SqlParameter("@CategoryIDs", categoryIDs));
                }
                // Add Keywords using stored procedure
                if (publicationDto.Keywords?.Any() == true)
                {
                    var keywordJson = JsonSerializer.Serialize(publicationDto.Keywords.Where(k => !string.IsNullOrWhiteSpace(k)));
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC sp_UpdatePublicationKeywords @PublicationID, @Keywords",
                        new SqlParameter("@PublicationID", publication.PublicationID),
                        new SqlParameter("@Keywords", keywordJson));
                }

                await transaction.CommitAsync();

                _context.ChangeTracker.Clear();

                // Fetch fresh data from database
                var refreshedPublication = await GetPublicationForEditAsync(publicationDto.PublicationID);

                if (refreshedPublication != null)
                {
                    return ServiceResult.Successful("Publication updated successfully.", refreshedPublication);
                }
                else
                {
                    // Still successful but couldn't get refreshed data
                    return ServiceResult.Successful("Publication updated successfully.");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error adding publication {PublicationId}", publicationDto.PublicationID);
                return ServiceResult.Failed("An error occurred while updating the publication.");
            }
        }
        //to here

        public async Task<PublicationRequestSubmissionDto> SubmitRequestAsync(PublicationRequestDto request)
        {
            //console.writeLine($"[{DateTime.Now:HH:mm:ss}] Request submission started");
            try
            {
                // In a real implementation, you would:
                // 1. Save the request to a database table (e.g., tblPublicationRequests)
                // 2. Send email notification to staff
                // 3. Send confirmation email to requester
                // 4. Generate PDF abstract if requested

                _logger.LogInformation("Publication request submitted: {Request}", System.Text.Json.JsonSerializer.Serialize(request));

                // For now, just simulate success
                await Task.Delay(100); // Simulate async operation
                //console.writeLine($"[{DateTime.Now:HH:mm:ss}] Request submitted successfully");
                return new PublicationRequestSubmissionDto
                {
                    Success = true,
                    Message = "Request submitted successfully",
                    RequestId = new Random().Next(1000, 9999) // In real app, this would be the actual DB ID
                };
            }
            catch (Exception ex)
            {
                //console.writeLine($"[{DateTime.Now:HH:mm:ss}] Request submitted successfully");
                _logger.LogError(ex, "Error submitting publication request");
                return new PublicationRequestSubmissionDto
                {
                    Success = false,
                    Message = "An error occurred while submitting your request. Please try again."
                };
            }
        }
    }
}