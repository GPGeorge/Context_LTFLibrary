using LTF_Library_V1.Data;
using LTF_Library_V1.DTOs;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

namespace LTF_Library_V1.Services
{
    public class MasterListService : IMasterListService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MasterListService> _logger;

        public MasterListService(ApplicationDbContext context, ILogger<MasterListService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Generic method for getting items
        public async Task<List<MasterListItemDto>> GetItemsAsync(string tableName)
        {
            return tableName switch
            {
                "Bookcase" => (await GetBookcasesAsync()).Select(b => new MasterListItemDto
                {
                    Id = b.BookcaseID,
                    Name = b.Bookcase1,
                    SortOrder = b.BookcaseID
                }).ToList(),
                
                "Genre" => (await GetGenresAsync()).Select(g => new MasterListItemDto
                {
                    Id = g.GenreID,
                    Name = g.Genre,
                    SortOrder = g.SortOrder
                }).ToList(),
                
                "MediaType" => (await GetMediaTypesAsync()).Select(m => new MasterListItemDto
                {
                    Id = m.MediaTypeID,
                    Name = m.MediaType,
                    SortOrder = m.SortOrder
                }).ToList(),
                
                "MediaCondition" => (await GetMediaConditionsAsync()).Select(m => new MasterListItemDto
                {
                    Id = m.MediaConditionID,
                    Name = m.MediaCondition,
                    SortOrder = m.SortOrder
                }).ToList(),
                
                "ParticipantStatus" => (await GetParticipantStatusesAsync()).Select(p => new MasterListItemDto
                {
                    Id = p.ParticipantStatusID,
                    Name = p.ParticipantStatus1,
                    SortOrder = p.SortOrder
                }).ToList(),
                
                _ => new List<MasterListItemDto>()
            };
        }

        // Specific getters
        public async Task<List<BookcaseDto>> GetBookcasesAsync()
        {
            return await _context.Set<Data.Models.Bookcase>()
                .OrderBy(b => b.Bookcase1)
                .ThenBy(b => b.BookcaseDescription)
                .Select(b => new BookcaseDto
                {
                    BookcaseID = b.BookcaseID,
                    Bookcase1 = b.Bookcase1,
                    BookcaseDescription = b.BookcaseDescription
                })
                .ToListAsync();
        }

        public async Task<List<CreatorDto>> GetCreatorsAsync()
        {
            return await _context.Set<Data.Models.Creator>()
                .OrderBy(c => c.CreatorLastName)
                .ThenBy(c => c.CreatorFirstName)
                .Select(c => new CreatorDto
                {
                    CreatorID = c.CreatorID,
                    CreatorFirstName = c.CreatorFirstName,
                    CreatorMiddleName = c.CreatorMiddleName,
                    CreatorLastName = c.CreatorLastName,
                    FullName = (c.CreatorFirstName ?? "") + 
                              (string.IsNullOrEmpty(c.CreatorMiddleName) ? " " : " " + c.CreatorMiddleName + " ") + 
                              (c.CreatorLastName ?? ""),
                    SortName = (c.CreatorLastName ?? "") + ", " + (c.CreatorFirstName ?? "")
                })
                .ToListAsync();
        }

        public async Task<List<GenreDto>> GetGenresAsync()
        {
            return await _context.Set<Data.Models.Genre>()
                .OrderBy(g => g.SortOrder ?? int.MaxValue)
                .ThenBy(g => g.Genre1)
                .Select(g => new GenreDto
                {
                    GenreID = g.GenreID,
                    Genre = g.Genre1,
                    SortOrder = g.SortOrder
                })
                .ToListAsync();
        }

        public async Task<List<PublisherDto>> GetPublishersAsync()
        {
            return await _context.Set<Data.Models.Publisher>()
                .OrderBy(p => p.Publisher1)
                .Select(p => new PublisherDto
                {
                    PublisherID = p.PublisherID,
                    Publisher1 = p.Publisher1,
                    PublisherGoogle = p.PublisherGoogle
                })
                .ToListAsync();
        }

        public async Task<List<MediaTypeDto>> GetMediaTypesAsync()
        {
            return await _context.Set<Data.Models.MediaType>()
                .OrderBy(m => m.SortOrder ?? int.MaxValue)
                .ThenBy(m => m.MediaType1)
                .Select(m => new MediaTypeDto
                {
                    MediaTypeID = m.MediaTypeID,
                    MediaType = m.MediaType1,
                    SortOrder = m.SortOrder
                })
                .ToListAsync();
        }

        public async Task<List<MediaConditionDto>> GetMediaConditionsAsync()
        {
            return await _context.Set<Data.Models.MediaCondition>()
                .OrderBy(m => m.SortOrder ?? int.MaxValue)
                .ThenBy(m => m.MediaCondition1)
                .Select(m => new MediaConditionDto
                {
                    MediaConditionID = m.MediaConditionID,
                    MediaCondition = m.MediaCondition1,
                    SortOrder = m.SortOrder
                })
                .ToListAsync();
        }

        public async Task<List<ParticipantDto>> GetParticipantsAsync()
        {
            return await _context.Set<Data.Models.Participant>()
                .OrderBy(p => p.ParticipantLastName)
                .ThenBy(p => p.ParticipantFirstName)
                .Select(p => new ParticipantDto
                {
                    ParticipantID = p.ParticipantID,
                    ParticipantFirstName = p.ParticipantFirstName,
                    ParticipantLastName = p.ParticipantLastName,
                    AlsoKnownAs = p.AlsoKnownAs,
                    FullName = (p.ParticipantFirstName ?? "") +                              
                              (p.ParticipantLastName ?? ""),
                    SortName = (p.ParticipantLastName ?? "") + ", " + (p.ParticipantFirstName ?? "")
                })
                .ToListAsync();
        }

        public async Task<List<ParticipantStatusDto>> GetParticipantStatusesAsync()
        {
            return await _context.Set<Data.Models.ParticipantStatus>()
                .OrderBy(p => p.SortOrder ?? int.MaxValue)
                .ThenBy(p => p.ParticipantStatus1)
                .Select(p => new ParticipantStatusDto
                {
                    ParticipantStatusID = p.ParticipantStatusID,
                    ParticipantStatus1 = p.ParticipantStatus1,
                    SortOrder = p.SortOrder
                })
                .ToListAsync();
        }

        public async Task<List<ShelfDto>> GetShelvesAsync()
        {
            return await _context.Set<Data.Models.Shelf>()
                .Include(s => s.Bookcase)
                .OrderBy(s => s.Bookcase.Bookcase1 )
                .ThenBy(s => s.Shelf1)
                .Select(s => new ShelfDto
                {
                    ShelfID = s.ShelfID,
                    Shelf1 = s.Shelf1,
                    BookcaseID = s.BookCaseID,
                    Bookcase1 = s.Bookcase != null ? s.Bookcase.Bookcase1 : null,
                    ShelfDescription = s.ShelfDescription
                })
                .ToListAsync();
        }

        // Add Item - Generic for simple tables
        public async Task<OperationResultDto> AddItemAsync(string tableName, MasterListItemDto item)
        {
            try
            {
                // Check for duplicates
                if (await CheckForDuplicateAsync(tableName, item.Name))
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = $"A {tableName} with this name already exists."
                    };
                }

                switch (tableName)
                {
                    case "Bookcase":
                        var bookcase = new Data.Models.Bookcase
                        {
                            Bookcase1 = item.Name,
                            BookcaseDescription = item.AdditionalField1
                        };
                        _context.Set<Data.Models.Bookcase>().Add(bookcase);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Bookcase added successfully.", AffectedId = bookcase.BookcaseID };
                    
                    case "Shelf":
                        var shelf = new Data.Models.Shelf
                        {
                            Shelf1 = item.Name,
                            ShelfDescription = item.AdditionalField1
                        };
                        _context.Set<Data.Models.Shelf>().Add(shelf);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Bookcase added successfully.", AffectedId = shelf.ShelfID };

                    case "Genre":
                        var genre = new Data.Models.Genre
                        {
                            Genre1 = item.Name,
                            SortOrder = item.SortOrder
                        };
                        _context.Set<Data.Models.Genre>().Add(genre);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Category added successfully.", AffectedId = genre.GenreID };

                    case "MediaType":
                        var mediaType = new Data.Models.MediaType
                        {
                            MediaType1 = item.Name,
                            SortOrder = item.SortOrder
                        };
                        _context.Set<Data.Models.MediaType>().Add(mediaType);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Media Type added successfully.", AffectedId = mediaType.MediaTypeID };

                    case "MediaCondition":
                        var mediaCondition = new Data.Models.MediaCondition
                        {
                            MediaCondition1 = item.Name,
                            SortOrder = item.SortOrder
                        };
                        _context.Set<Data.Models.MediaCondition>().Add(mediaCondition);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Media Condition added successfully.", AffectedId = mediaCondition.MediaConditionID };

                    case "ParticipantStatus":
                        var participantStatus = new Data.Models.ParticipantStatus
                        {
                            ParticipantStatus1 = item.Name,
                            SortOrder = item.SortOrder
                        };
                        _context.Set<Data.Models.ParticipantStatus>().Add(participantStatus);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Participant Status added successfully.", AffectedId = participantStatus.ParticipantStatusID };

                    default:
                        return new OperationResultDto { Success = false, Message = "Invalid table name." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding item to {tableName}");
                return new OperationResultDto { Success = false, Message = $"Error adding item: {ex.Message}" };
            }
        }

        // Specific Add methods for complex tables
        public async Task<OperationResultDto> AddCreatorAsync(CreatorDto creator)
        {
            try
            {
                // Check for duplicate name combination
                if (await _context.Set<Data.Models.Creator>().AnyAsync(c => 
                    c.CreatorFirstName == creator.CreatorFirstName &&
                    c.CreatorLastName == creator.CreatorLastName &&
                    c.CreatorMiddleName == creator.CreatorMiddleName))
                {
                    return new OperationResultDto { Success = false, Message = "An author with this name already exists." };
                }

                var newCreator = new Data.Models.Creator
                {
                    CreatorFirstName = creator.CreatorFirstName,
                    CreatorMiddleName = creator.CreatorMiddleName,
                    CreatorLastName = creator.CreatorLastName
                };
                
                _context.Set<Data.Models.Creator>().Add(newCreator);
                await _context.SaveChangesAsync();
                
                return new OperationResultDto { Success = true, Message = "Author added successfully.", AffectedId = newCreator.CreatorID };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding creator");
                return new OperationResultDto { Success = false, Message = $"Error adding author: {ex.Message}" };
            }
        }

        public async Task<OperationResultDto> AddPublisherAsync(PublisherDto publisher)
        {
            try
            {
                if (await _context.Set<Data.Models.Publisher>().AnyAsync(p => 
                    p.Publisher1.ToLower() == publisher.Publisher1.ToLower()))
                {
                    return new OperationResultDto { Success = false, Message = "A publisher with this name already exists." };
                }

                var newPublisher = new Data.Models.Publisher
                {
                    Publisher1 = publisher.Publisher1,
                    PublisherGoogle = publisher.PublisherGoogle
                };
                
                _context.Set<Data.Models.Publisher>().Add(newPublisher);
                await _context.SaveChangesAsync();
                
                return new OperationResultDto { Success = true, Message = "Publisher added successfully.", AffectedId = newPublisher.PublisherID };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding publisher");
                return new OperationResultDto { Success = false, Message = $"Error adding publisher: {ex.Message}" };
            }
        }

        public async Task<OperationResultDto> AddParticipantAsync(ParticipantDto participant)
        {
            try
            {
                if (await _context.Set<Data.Models.Participant>().AnyAsync(p => 
                    p.ParticipantFirstName == participant.ParticipantFirstName &&
                    p.ParticipantLastName == participant.ParticipantLastName))
                {
                    return new OperationResultDto { Success = false, Message = "A participant with this name already exists." };
                }

                var newParticipant = new Data.Models.Participant
                {
                    ParticipantFirstName = participant.ParticipantFirstName,
                    ParticipantLastName = participant.ParticipantLastName,
                    AlsoKnownAs=participant.AlsoKnownAs
                };
                
                _context.Set<Data.Models.Participant>().Add(newParticipant);
                await _context.SaveChangesAsync();
                
                return new OperationResultDto { Success = true, Message = "Participant added successfully.", AffectedId = newParticipant.ParticipantID };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding participant");
                return new OperationResultDto { Success = false, Message = $"Error adding participant: {ex.Message}" };
            }
        }

        public async Task<OperationResultDto> AddShelfAsync(ShelfDto shelf)
        {
            try
            {
                // Check for duplicate shelf name within the same bookcase
                if (await _context.Set<Data.Models.Shelf>().AnyAsync(s => 
                    s.Shelf1.ToLower() == shelf.Shelf1.ToLower() && 
                    s.BookCaseID == shelf.BookcaseID))
                {
                    return new OperationResultDto { Success = false, Message = "A shelf with this name already exists in this bookcase." };
                }

                var newShelf = new Data.Models.Shelf
                {
                    Shelf1 = shelf.Shelf1,
                    BookCaseID = shelf.BookcaseID,
                    ShelfDescription = shelf.ShelfDescription
                };
                
                _context.Set<Data.Models.Shelf>().Add(newShelf);
                await _context.SaveChangesAsync();
                
                return new OperationResultDto { Success = true, Message = "Shelf added successfully.", AffectedId = newShelf.ShelfID };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding shelf");
                return new OperationResultDto { Success = false, Message = $"Error adding shelf: {ex.Message}" };
            }
        }

        // Update Item - Generic for simple tables
        public async Task<OperationResultDto> UpdateItemAsync(string tableName, MasterListItemDto item)
        {
            try
            {
                // Check for duplicates (excluding the current item)
                if (await CheckForDuplicateAsync(tableName, item.Name, item.Id))
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = $"A {tableName} with this name already exists."
                    };
                }

                switch (tableName)
                {
                    case "Bookcase":
                        var bookcase = await _context.Set<Data.Models.Bookcase>().FindAsync(item.Id);
                        if (bookcase == null) return new OperationResultDto { Success = false, Message = "Bookcase not found." };
                        bookcase.Bookcase1 = item.Name;
                        bookcase.BookcaseDescription = item.AdditionalField1;
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Bookcase updated successfully." };

                    case "Shelf":
                        var shelf = await _context.Set<Data.Models.Shelf>().FindAsync(item.Id);
                        if (shelf == null)
                            return new OperationResultDto { Success = false, Message = "Shelf not found." };
                        shelf.Shelf1 = item.Name;
                        shelf.BookCaseID = item?.RelatedId ?? shelf.BookCaseID;
                        shelf.ShelfDescription = item?.AdditionalField1;
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Shelf updated successfully." };

                    case "Genre":
                        var genre = await _context.Set<Data.Models.Genre>().FindAsync(item.Id);
                        if (genre == null) return new OperationResultDto { Success = false, Message = "Category not found." };
                        genre.Genre1 = item.Name;
                        genre.SortOrder = item.SortOrder;
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Category updated successfully." };

                    case "MediaType":
                        var mediaType = await _context.Set<Data.Models.MediaType>().FindAsync(item.Id);
                        if (mediaType == null) return new OperationResultDto { Success = false, Message = "Media Type not found." };
                        mediaType.MediaType1 = item.Name;
                        mediaType.SortOrder = item.SortOrder;
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Media Type updated successfully." };

                    case "MediaCondition":
                        var mediaCondition = await _context.Set<Data.Models.MediaCondition>().FindAsync(item.Id);
                        if (mediaCondition == null) return new OperationResultDto { Success = false, Message = "Media Condition not found." };
                        mediaCondition.MediaCondition1 = item.Name;
                        mediaCondition.SortOrder = item.SortOrder;
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Media Condition updated successfully." };

                    case "ParticipantStatus":
                        var participantStatus = await _context.Set<Data.Models.ParticipantStatus>().FindAsync(item.Id);
                        if (participantStatus == null) return new OperationResultDto { Success = false, Message = "Participant Status not found." };
                        participantStatus.ParticipantStatus1 = item.Name;
                        participantStatus.SortOrder = item.SortOrder;
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Participant Status updated successfully." };

                    default:
                        return new OperationResultDto { Success = false, Message = "Invalid table name." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating item in {tableName}");
                return new OperationResultDto { Success = false, Message = $"Error updating item: {ex.Message}" };
            }
        }

        // Specific Update methods for complex tables
        public async Task<OperationResultDto> UpdateCreatorAsync(CreatorDto creator)
        {
            try
            {
                var existing = await _context.Set<Data.Models.Creator>().FindAsync(creator.CreatorID);
                if (existing == null) return new OperationResultDto { Success = false, Message = "Author not found." };

                // Check for duplicate (excluding current)
                if (await _context.Set<Data.Models.Creator>().AnyAsync(c => 
                    c.CreatorID != creator.CreatorID &&
                    c.CreatorFirstName == creator.CreatorFirstName &&
                    c.CreatorLastName == creator.CreatorLastName &&
                    c.CreatorMiddleName == creator.CreatorMiddleName))
                {
                    return new OperationResultDto { Success = false, Message = "An author with this name already exists." };
                }

                existing.CreatorFirstName = creator.CreatorFirstName;
                existing.CreatorMiddleName = creator.CreatorMiddleName;
                existing.CreatorLastName = creator.CreatorLastName;
                
                await _context.SaveChangesAsync();
                return new OperationResultDto { Success = true, Message = "Author updated successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating creator");
                return new OperationResultDto { Success = false, Message = $"Error updating author: {ex.Message}" };
            }
        }

        public async Task<OperationResultDto> UpdatePublisherAsync(PublisherDto publisher)
        {
            try
            {
                var existing = await _context.Set<Data.Models.Publisher>().FindAsync(publisher.PublisherID);
                if (existing == null) return new OperationResultDto { Success = false, Message = "Publisher not found." };

                if (await _context.Set<Data.Models.Publisher>().AnyAsync(p => 
                    p.PublisherID != publisher.PublisherID &&
                    p.Publisher1.ToLower() == publisher.Publisher1.ToLower()))
                {
                    return new OperationResultDto { Success = false, Message = "A publisher with this name already exists." };
                }

                existing.Publisher1 = publisher.Publisher1;
                existing.PublisherGoogle = publisher.PublisherGoogle;
                
                await _context.SaveChangesAsync();
                return new OperationResultDto { Success = true, Message = "Publisher updated successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating publisher");
                return new OperationResultDto { Success = false, Message = $"Error updating publisher: {ex.Message}" };
            }
        }

        public async Task<OperationResultDto> UpdateParticipantAsync(ParticipantDto participant)
        {
            try
            {
                var existing = await _context.Set<Data.Models.Participant>().FindAsync(participant.ParticipantID);
                if (existing == null) return new OperationResultDto { Success = false, Message = "Participant not found." };

                if (await _context.Set<Data.Models.Participant>().AnyAsync(p => 
                    p.ParticipantID != participant.ParticipantID &&
                    p.ParticipantFirstName == participant.ParticipantFirstName &&
                    p.ParticipantLastName == participant.ParticipantLastName))
                {
                    return new OperationResultDto { Success = false, Message = "A participant with this name already exists." };
                }

                existing.ParticipantFirstName = participant.ParticipantFirstName;
                existing.ParticipantLastName = participant.ParticipantLastName;
                existing.AlsoKnownAs = participant.AlsoKnownAs;

                await _context.SaveChangesAsync();
                return new OperationResultDto { Success = true, Message = "Participant updated successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating participant");
                return new OperationResultDto { Success = false, Message = $"Error updating participant: {ex.Message}" };
            }
        }

        public async Task<OperationResultDto> UpdateShelfAsync(ShelfDto shelf)
        {
            try
            {
                var existing = await _context.Set<Data.Models.Shelf>().FindAsync(shelf.ShelfID);
                if (existing == null) return new OperationResultDto { Success = false, Message = "Shelf not found." };

                if (await _context.Set<Data.Models.Shelf>().AnyAsync(s => 
                    s.ShelfID != shelf.ShelfID &&
                    s.Shelf1.ToLower()  == shelf.Shelf1.ToLower() && 
                    s.BookCaseID == shelf.BookcaseID))
                {
                    return new OperationResultDto { Success = false, Message = "A shelf with this name already exists in this bookcase." };
                }
                existing.Shelf1 = shelf.Shelf1;
                existing.BookCaseID = shelf.BookcaseID;
                existing.ShelfDescription = shelf.ShelfDescription;
                
                await _context.SaveChangesAsync();
                return new OperationResultDto { Success = true, Message = "Shelf updated successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shelf");
                return new OperationResultDto { Success = false, Message = $"Error updating shelf: {ex.Message}" };
            }
        }

        // Delete Item
        public async Task<OperationResultDto> DeleteItemAsync(string tableName, int itemId)
        {
            try
            {
                // Check if item is in use
                if (await IsItemInUseAsync(tableName, itemId))
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = $"This {tableName} cannot be deleted because it is currently in use by one or more publications or related records."
                    };
                }

                switch (tableName)
                {
                    case "Bookcase":
                        var bookcase = await _context.Set<Data.Models.Bookcase>().FindAsync(itemId);
                        if (bookcase == null) return new OperationResultDto { Success = false, Message = "Bookcase not found." };
                        _context.Set<Data.Models.Bookcase>().Remove(bookcase);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Bookcase deleted successfully." };

                    case "Creator":
                        var creator = await _context.Set<Data.Models.Creator>().FindAsync(itemId);
                        if (creator == null) return new OperationResultDto { Success = false, Message = "Author not found." };
                        _context.Set<Data.Models.Creator>().Remove(creator);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Author deleted successfully." };

                    case "Genre":
                        var genre = await _context.Set<Data.Models.Genre>().FindAsync(itemId);
                        if (genre == null) return new OperationResultDto { Success = false, Message = "Category not found." };
                        _context.Set<Data.Models.Genre>().Remove(genre);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Category deleted successfully." };

                    case "Publisher":
                        var publisher = await _context.Set<Data.Models.Publisher>().FindAsync(itemId);
                        if (publisher == null) return new OperationResultDto { Success = false, Message = "Publisher not found." };
                        _context.Set<Data.Models.Publisher>().Remove(publisher);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Publisher deleted successfully." };

                    case "MediaType":
                        var mediaType = await _context.Set<Data.Models.MediaType>().FindAsync(itemId);
                        if (mediaType == null) return new OperationResultDto { Success = false, Message = "Media Type not found." };
                        _context.Set<Data.Models.MediaType>().Remove(mediaType);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Media Type deleted successfully." };

                    case "MediaCondition":
                        var mediaCondition = await _context.Set<Data.Models.MediaCondition>().FindAsync(itemId);
                        if (mediaCondition == null) return new OperationResultDto { Success = false, Message = "Media Condition not found." };
                        _context.Set<Data.Models.MediaCondition>().Remove(mediaCondition);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Media Condition deleted successfully." };

                    case "Participant":
                        var participant = await _context.Set<Data.Models.Participant>().FindAsync(itemId);
                        if (participant == null) return new OperationResultDto { Success = false, Message = "Participant not found." };
                        _context.Set<Data.Models.Participant>().Remove(participant);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Participant deleted successfully." };

                    case "ParticipantStatus":
                        var participantStatus = await _context.Set<Data.Models.ParticipantStatus>().FindAsync(itemId);
                        if (participantStatus == null) return new OperationResultDto { Success = false, Message = "Participant Status not found." };
                        _context.Set<Data.Models.ParticipantStatus>().Remove(participantStatus);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Participant Status deleted successfully." };

                    case "Shelf":
                        var shelf = await _context.Set<Data.Models.Shelf>().FindAsync(itemId);
                        if (shelf == null) return new OperationResultDto { Success = false, Message = "Shelf not found." };
                        _context.Set<Data.Models.Shelf>().Remove(shelf);
                        await _context.SaveChangesAsync();
                        return new OperationResultDto { Success = true, Message = "Shelf deleted successfully." };

                    default:
                        return new OperationResultDto { Success = false, Message = "Invalid table name." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting item from {tableName}");
                return new OperationResultDto { Success = false, Message = $"Error deleting item: {ex.Message}" };
            }
        }

        // Check for duplicates
        public async Task<bool> CheckForDuplicateAsync(string tableName, string value, int? excludeId = null)
        {
            try
            {
                return tableName switch
                {
                    "Bookcase" => await _context.Set<Data.Models.Bookcase>()
                        .AnyAsync(b => b.Bookcase1.ToLower() == value.ToLower() && (excludeId == null || b.BookcaseID != excludeId)),
                    
                    "Genre" => await _context.Set<Data.Models.Genre>()
                        .AnyAsync(g => g.Genre1.ToLower() == value.ToLower() && (excludeId == null || g.GenreID != excludeId)),
                    
                    "MediaType" => await _context.Set<Data.Models.MediaType>()
                        .AnyAsync(m => m.MediaType1.ToLower() == value.ToLower() && (excludeId == null || m.MediaTypeID != excludeId)),
                    
                    "MediaCondition" => await _context.Set<Data.Models.MediaCondition>()
                        .AnyAsync(m => m.MediaCondition1.ToLower() == value.ToLower() && (excludeId == null || m.MediaConditionID != excludeId)),
                    
                    "ParticipantStatus" => await _context.Set<Data.Models.ParticipantStatus>()
                        .AnyAsync(p => p.ParticipantStatus1.ToLower() == value.ToLower() && (excludeId == null || p.ParticipantStatusID != excludeId)),
                    
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking for duplicate in {tableName}");
                return false;
            }
        }

        // Check if item is in use
        public async Task<bool> IsItemInUseAsync(string tableName, int itemId)
        {
            try
            {
                return tableName switch
                {
                    "Bookcase" => await _context.Set<Data.Models.Shelf>().AnyAsync(s => s.ShelfID == itemId),
                    
                    "Creator" => await _context.Set<Data.Models.PublicationCreator>().AnyAsync(pc => pc.CreatorID == itemId),
                    
                    "Genre" => await _context.Set<Data.Models.PublicationGenre>().AnyAsync(pg => pg.GenreID == itemId),
                    
                    "Publisher" => await _context.Set<Data.Models.Publication>().AnyAsync(p => p.PublisherID == itemId),
                    
                    "MediaType" => await _context.Set<Data.Models.Publication>().AnyAsync(p => p.MediaTypeID == itemId),
                    
                    "MediaCondition" => await _context.Set<Data.Models.Publication>().AnyAsync(p => p.MediaConditionID == itemId),
                    
                    "Participant" => await _context.Set<Data.Models.PublicationTransfer>().AnyAsync(pp => pp.ParticipantID == itemId),
                    
                    "ParticipantStatus" => await _context.Set<Data.Models.PublicationTransfer>()
                        .AnyAsync(pp => pp.ParticipantStatusID == itemId),
                    
                    "Shelf" => await _context.Set<Data.Models.Publication>().AnyAsync(p => p.ShelfID == itemId),
                    
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if item in use for {tableName}");
                return false;
            }
        }
    }
}