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