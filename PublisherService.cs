// File: Services/PublisherService.cs
using LTF_Library_V1.Data;
using LTF_Library_V1.Data.Models;
using LTF_Library_V1.DTOs;
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

                if (publisher == null)
                    return null;

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