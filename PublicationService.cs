using LTF_Library_V1.Data;
using LTF_Library_V1.Data.Models;
using LTF_Library_V1.DTOs;
using LTF_Library_V1.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
                    .Include(p => p.PublicationKeyWords)
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

                // Apply keyword filter
                if (!string.IsNullOrWhiteSpace(request.Criteria.Keyword))
                {
                    query = query.Where(p => p.PublicationKeyWords.Any(pkw => pkw.KeyWord!.Contains(request.Criteria.Keyword)));
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

        public async Task<List<CreatorDto>> GetAuthorsAsync()
        {
            try
            {
                return await _context.Creators
                    .Where(c => c.PublicationCreators.Any()) // Only authors with publications
                    .OrderBy(c => c.CreatorLastName)
                    .ThenBy(c => c.CreatorFirstName)
                    .Select(c => new CreatorDto
                    {
                        CreatorID = c.CreatorID,
                        CreatorFirstName = c.CreatorFirstName,
                        CreatorMiddleName = c.CreatorMiddleName,
                        CreatorLastName = c.CreatorLastName,
                        FullName = c.FullName,
                        SortName = c.SortName
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authors");
                throw;
            }
        }

        public async Task<List<GenreDto>> GetGenresAsync()
        {
            try
            {
                return await _context.Genres
                    .Where(g => g.PublicationGenres.Any()) // Only genres with publications
                    .OrderBy(g => g.SortOrder)
                    .ThenBy(g => g.Genre1)
                    .Select(g => new GenreDto
                    {
                        GenreID = g.GenreID,
                        Genre = g.Genre1!,
                        SortOrder = g.SortOrder
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting genres");
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

        public async Task<PublicationEditDto?> GetPublicationForEditAsync(int publicationId)
        {
            try
            {
                var publication = await _context.Publications
                    .Include(p => p.PublicationCreators)
                        .ThenInclude(pc => pc.Creator)
                    .Include(p => p.PublicationGenres)
                        .ThenInclude(pg => pg.Genre)
                    .Include(p => p.PublicationKeyWords)
                    .Include(p => p.MediaType)
                    .Include(p => p.MediaCondition)
                    .Include(p => p.Publisher)
                    .Include(p => p.Shelf)
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
                    Printing = publication.Printing,
                    YearPublished = publication.YearPublished,
                    ConfidenceLevel = publication.ConfidenceLevel,
                    ListPrice = publication.ListPrice,
                    PublisherID = publication.PublisherID,
                    MediaTypeID = publication.MediaTypeID,
                    MediaConditionID = publication.MediaConditionID,
                    ShelfID = publication.ShelfID,
                    AuthorIds = publication.PublicationCreators
                        .Where(pc => pc.CreatorID.HasValue)
                        .Select(pc => pc.CreatorID!.Value)
                        .ToList(),
                    GenreIds = publication.PublicationGenres
                        .Where(pg => pg.GenreID.HasValue)
                        .Select(pg => pg.GenreID!.Value)
                        .ToList(),
                    Keywords = publication.PublicationKeyWords.Select(pkw => pkw.KeyWord!).ToList(),
                    SelectedAuthors = publication.PublicationCreators
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
                        .Select(pg => new GenreDto
                        {
                            GenreID = pg.Genre!.GenreID,
                            Genre = pg.Genre.Genre1!,
                            SortOrder = pg.Genre.SortOrder
                        }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting publication for edit: {PublicationId}", publicationId);
                throw;
            }
        }

        ////new 
        public async Task<ServiceResult> UpdatePublicationAsync(PublicationEditDto publication)
        {
            // Use a transaction to ensure all changes succeed or all fail
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ============================================================
                // DIAGNOSTIC LOGGING - See what data we received
                // ============================================================
                _logger.LogInformation("=== RECEIVED DATA FOR Publication {PublicationId} ===", publication.PublicationID);
                _logger.LogInformation("AuthorIds: {AuthorIds}",
                    publication.AuthorIds == null ? "NULL" : string.Join(", ", publication.AuthorIds));
                _logger.LogInformation("GenreIds: {GenreIds}",
                    publication.GenreIds == null ? "NULL" : string.Join(", ", publication.GenreIds));
                _logger.LogInformation("Keywords: {Keywords}",
                    publication.Keywords == null ? "NULL" : string.Join(", ", publication.Keywords));
                _logger.LogInformation("=== END RECEIVED DATA ===");

                var existingPublication = await _context.Publications
                    .FirstOrDefaultAsync(p => p.PublicationID == publication.PublicationID);

                if (existingPublication == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Publication not found"
                    };
                }

                // Update basic properties
                existingPublication.PublicationTitle = publication.PublicationTitle;
                existingPublication.CatalogNumber = publication.CatalogNumber;
                existingPublication.Comments = publication.Comments;
                existingPublication.CoverPhotoLink = publication.CoverPhotoLink;
                existingPublication.Edition = publication.Edition;
                existingPublication.ISBN = publication.ISBN;
                existingPublication.Volume = publication.Volume;
                existingPublication.NumberOfVolumes = publication.NumberOfVolumes;
                existingPublication.Pages = publication.Pages;
                existingPublication.Printing = publication.Printing;
                existingPublication.YearPublished = publication.YearPublished;
                existingPublication.ConfidenceLevel = publication.ConfidenceLevel;
                existingPublication.ListPrice = publication.ListPrice;
                existingPublication.PublisherID = publication.PublisherID;
                existingPublication.MediaTypeID = publication.MediaTypeID;
                existingPublication.MediaConditionID = publication.MediaConditionID;
                existingPublication.ShelfID = publication.ShelfID;

                _logger.LogInformation("Starting update for Publication {PublicationId}", publication.PublicationID);

                // ============================================================
                // Update PublicationCreators (Authors)
                // ============================================================

                // Load existing creator entities with tracking
                var existingCreatorEntities = await _context.PublicationCreators
                    .Where(pc => pc.PublicationID == publication.PublicationID)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} existing PublicationCreators for Publication {PublicationId}",
                    existingCreatorEntities.Count, publication.PublicationID);

                var existingAuthorIds = existingCreatorEntities
                    .Where(pc => pc.CreatorID.HasValue)
                    .Select(pc => pc.CreatorID!.Value)
                    .ToList();

                _logger.LogInformation("Existing CreatorIDs in DB: {ExistingIds}", string.Join(", ", existingAuthorIds));

                var newAuthorIds = publication.AuthorIds ?? new List<int>();

                _logger.LogInformation("New CreatorIDs from form: {NewIds}", string.Join(", ", newAuthorIds));

                // Find creators to remove (in existing but not in new)
                var creatorsToRemove = existingCreatorEntities
                    .Where(pc => pc.CreatorID.HasValue && !newAuthorIds.Contains(pc.CreatorID.Value))
                    .ToList();

                // Find author IDs to add (in new but not in existing)
                var authorIdsToAdd = newAuthorIds
                    .Where(id => !existingAuthorIds.Contains(id))
                    .ToList();

                _logger.LogInformation("PublicationCreators: Removing {RemoveCount}, Adding {AddCount}",
                    creatorsToRemove.Count, authorIdsToAdd.Count);

                if (creatorsToRemove.Any())
                {
                    _logger.LogInformation("IDs being removed: {RemoveIds}",
                        string.Join(", ", creatorsToRemove.Select(c => c.CreatorID)));
                }

                if (authorIdsToAdd.Any())
                {
                    _logger.LogInformation("IDs being added: {AddIds}", string.Join(", ", authorIdsToAdd));
                }

                // Remove creators that are no longer associated
                if (creatorsToRemove.Any())
                {
                    _context.PublicationCreators.RemoveRange(creatorsToRemove);
                    _logger.LogInformation("Marked {Count} PublicationCreators for deletion", creatorsToRemove.Count);
                }

                // Add new creators
                if (authorIdsToAdd.Any())
                {
                    var newCreators = authorIdsToAdd.Select(authorId => new PublicationCreator
                    {
                        PublicationID = publication.PublicationID,
                        CreatorID = authorId
                    }).ToList();

                    await _context.PublicationCreators.AddRangeAsync(newCreators);
                    _logger.LogInformation("Added {Count} new PublicationCreators", newCreators.Count);
                }

                // ============================================================
                // Update PublicationGenres (Categories)
                // ============================================================

                // Load existing genre entities with tracking
                var existingGenreEntities = await _context.PublicationGenres
                    .Where(pg => pg.PublicationID == publication.PublicationID)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} existing PublicationGenres for Publication {PublicationId}",
                    existingGenreEntities.Count, publication.PublicationID);

                var existingGenreIds = existingGenreEntities
                    .Where(pg => pg.GenreID.HasValue)
                    .Select(pg => pg.GenreID!.Value)
                    .ToList();

                _logger.LogInformation("Existing GenreIDs in DB: {ExistingIds}", string.Join(", ", existingGenreIds));

                var newGenreIds = publication.GenreIds ?? new List<int>();

                _logger.LogInformation("New GenreIDs from form: {NewIds}", string.Join(", ", newGenreIds));

                // Find genres to remove (in existing but not in new)
                var genresToRemove = existingGenreEntities
                    .Where(pg => pg.GenreID.HasValue && !newGenreIds.Contains(pg.GenreID.Value))
                    .ToList();

                // Find genre IDs to add (in new but not in existing)
                var genreIdsToAdd = newGenreIds
                    .Where(id => !existingGenreIds.Contains(id))
                    .ToList();

                _logger.LogInformation("PublicationGenres: Removing {RemoveCount}, Adding {AddCount}",
                    genresToRemove.Count, genreIdsToAdd.Count);

                if (genresToRemove.Any())
                {
                    _logger.LogInformation("IDs being removed: {RemoveIds}",
                        string.Join(", ", genresToRemove.Select(g => g.GenreID)));
                }

                if (genreIdsToAdd.Any())
                {
                    _logger.LogInformation("IDs being added: {AddIds}", string.Join(", ", genreIdsToAdd));
                }

                // Remove genres that are no longer associated
                if (genresToRemove.Any())
                {
                    _context.PublicationGenres.RemoveRange(genresToRemove);
                    _logger.LogInformation("Marked {Count} PublicationGenres for deletion", genresToRemove.Count);
                }

                // Add new genres
                if (genreIdsToAdd.Any())
                {
                    var newGenres = genreIdsToAdd.Select(genreId => new PublicationGenre
                    {
                        PublicationID = publication.PublicationID,
                        GenreID = genreId
                    }).ToList();

                    await _context.PublicationGenres.AddRangeAsync(newGenres);
                    _logger.LogInformation("Added {Count} new PublicationGenres", newGenres.Count);
                }

                // ============================================================
                // Update PublicationKeyWords
                // ============================================================

                // Load existing keywords for this publication
                var existingKeywordEntities = await _context.PublicationKeyWords
                    .Where(pkw => pkw.PublicationID == publication.PublicationID)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} existing PublicationKeyWords for Publication {PublicationId}",
                    existingKeywordEntities.Count, publication.PublicationID);

                var existingKeywords = existingKeywordEntities
                    .Select(pkw => pkw.KeyWord ?? "")
                    .ToList();

                var newKeywords = publication.Keywords ?? new List<string>();

                // Find keywords to remove (in existing but not in new)
                var keywordsToRemove = existingKeywordEntities
                    .Where(pkw => !newKeywords.Contains(pkw.KeyWord ?? ""))
                    .ToList();

                // Find keywords to add (in new but not in existing)
                var keywordsToAdd = newKeywords
                    .Where(kw => !string.IsNullOrWhiteSpace(kw) && !existingKeywords.Contains(kw))
                    .ToList();

                _logger.LogInformation("PublicationKeyWords: Removing {RemoveCount}, Adding {AddCount}",
                    keywordsToRemove.Count, keywordsToAdd.Count);

                // Remove keywords that are no longer associated
                if (keywordsToRemove.Any())
                {
                    _context.PublicationKeyWords.RemoveRange(keywordsToRemove);
                    _logger.LogInformation("Marked {Count} PublicationKeyWords for deletion", keywordsToRemove.Count);
                }

                // Add new keywords
                if (keywordsToAdd.Any())
                {
                    var newKeywordEntities = keywordsToAdd.Select(keyword => new PublicationKeyWord
                    {
                        PublicationID = publication.PublicationID,
                        KeyWord = keyword
                    }).ToList();

                    await _context.PublicationKeyWords.AddRangeAsync(newKeywordEntities);
                    _logger.LogInformation("Added {Count} new PublicationKeyWords", newKeywordEntities.Count);
                }

                // ============================================================
                // Save all changes within the transaction
                // ============================================================
                _logger.LogInformation("Saving changes for Publication {PublicationId}", publication.PublicationID);

                var changeCount = await _context.SaveChangesAsync();

                _logger.LogInformation("SaveChanges completed with {ChangeCount} changes", changeCount);

                // Commit the transaction
                await transaction.CommitAsync();

                _logger.LogInformation("Transaction committed successfully for Publication {PublicationId}", publication.PublicationID);

                return new ServiceResult
                {
                    Success = true,
                    Message = "Publication updated successfully"
                };
            }
            catch (Exception ex)
            {
                // Rollback the transaction if anything fails
                await transaction.RollbackAsync();

                _logger.LogError(ex, "Error updating publication {PublicationId}. Exception: {Message}",
                    publication.PublicationID, ex.Message);

                return new ServiceResult
                {
                    Success = false,
                    Message = $"An error occurred while updating the publication: {ex.Message}"
                };
            }
        }
        ////end 
        

        public async Task<ServiceResult> CreatePublicationAsync(PublicationEditDto publication)
        {
            try
            {
                var newPublication = new Publication
                {
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
                    ListPrice = publication.ListPrice,
                    PublisherID = publication.PublisherID,
                    MediaTypeID = publication.MediaTypeID,
                    MediaConditionID = publication.MediaConditionID,
                    ShelfID = publication.ShelfID,
                    DateCaptured = DateTime.Now
                };

                _context.Publications.Add(newPublication);
                await _context.SaveChangesAsync();

                // Add creators (authors)
                foreach (var authorId in publication.AuthorIds)
                {
                    _context.PublicationCreators.Add(new PublicationCreator
                    {
                        PublicationID = newPublication.PublicationID,
                        CreatorID = authorId
                    });
                }

                // Add genres (categories)
                foreach (var genreId in publication.GenreIds)
                {
                    _context.PublicationGenres.Add(new PublicationGenre
                    {
                        PublicationID = newPublication.PublicationID,
                        GenreID = genreId
                    });
                }

                // Add keywords
                foreach (var keyword in publication.Keywords)
                {
                    _context.PublicationKeyWords.Add(new PublicationKeyWord
                    {
                        PublicationID = newPublication.PublicationID,
                        KeyWord = keyword
                    });
                }

                await _context.SaveChangesAsync();

                return new ServiceResult
                {
                    Success = true,
                    Message = "Publication created successfully",
                    Data = newPublication.PublicationID
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating publication");
                return new ServiceResult
                {
                    Success = false,
                    Message = "An error occurred while creating the publication"
                };
            }
        }

        public async Task<List<string>> GetAllKeywordsAsync()
        {
            try
            {
                // Get distinct keywords from tblPublicationKeyWords that are actually used in publications
                return await _context.PublicationKeyWords
                    .Where(pkw => pkw.KeyWord != null && pkw.KeyWord != "")
                    .Select(pkw => pkw.KeyWord!)
                    .Distinct()
                    .OrderBy(kw => kw)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all keywords");
                throw;
            }
        }

        public async Task<List<string>> GetKeywordsForPublicationAsync(int publicationId)
        {
            try
            {
                return await _context.PublicationKeyWords
                    .Where(pkw => pkw.PublicationID == publicationId && pkw.KeyWord != null)
                    .Select(pkw => pkw.KeyWord!)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting keywords for publication {PublicationId}", publicationId);
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
                        Publisher = p.Publisher1!,
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
    }
}