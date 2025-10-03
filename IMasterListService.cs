using LTF_Library_V1.DTOs;

namespace LTF_Library_V1.Services
{
    public interface IMasterListService
    {
        // Generic methods for simple master lists
        Task<List<MasterListItemDto>> GetItemsAsync(string tableName);
        Task<OperationResultDto> AddItemAsync(string tableName, MasterListItemDto item);
        Task<OperationResultDto> UpdateItemAsync(string tableName, MasterListItemDto item);
        Task<OperationResultDto> DeleteItemAsync(string tableName, int itemId);

        // Specific getters for each table type
        Task<List<BookcaseDto>> GetBookcasesAsync();
        Task<List<CreatorDto>> GetCreatorsAsync();
        Task<List<GenreDto>> GetGenresAsync();
        Task<List<PublisherDto>> GetPublishersAsync();
        Task<List<MediaTypeDto>> GetMediaTypesAsync();
        Task<List<MediaConditionDto>> GetMediaConditionsAsync();
        Task<List<ParticipantDto>> GetParticipantsAsync();
        Task<List<ParticipantStatusDto>> GetParticipantStatusesAsync();
        Task<List<ShelfDto>> GetShelvesAsync();

        // Specific CRUD methods for complex tables
        Task<OperationResultDto> AddCreatorAsync(CreatorDto creator);
        Task<OperationResultDto> UpdateCreatorAsync(CreatorDto creator);

        Task<OperationResultDto> AddPublisherAsync(PublisherDto publisher);
        Task<OperationResultDto> UpdatePublisherAsync(PublisherDto publisher);

        Task<OperationResultDto> AddParticipantAsync(ParticipantDto participant);
        Task<OperationResultDto> UpdateParticipantAsync(ParticipantDto participant);

        Task<OperationResultDto> AddShelfAsync(ShelfDto shelf);
        Task<OperationResultDto> UpdateShelfAsync(ShelfDto shelf);

        // Validation methods
        Task<bool> CheckForDuplicateAsync(string tableName, string value, int? excludeId = null);
        Task<bool> IsItemInUseAsync(string tableName, int itemId);
    }
}