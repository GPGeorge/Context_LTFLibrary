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
