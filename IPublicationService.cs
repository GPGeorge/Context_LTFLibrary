using LTF_Library_V1.DTOs;

namespace LTF_Library_V1.Services
{
    public interface IPublicationService
    {
        Task<PublicationSearchResponse> SearchPublicationsAsync(PublicationSearchRequest request);
        Task<PublicationDetailDto?> GetPublicationDetailAsync(int publicationId);
        Task<List<CreatorDto>> GetAuthorsAsync();
        Task<List<GenreDto>> GetGenresAsync();
        Task<List<MediaTypeDto>> GetMediaTypesAsync();
        Task<CollectionStatisticsDto> GetCollectionStatisticsAsync();
        Task<PublicationRequestSubmissionDto> SubmitRequestAsync(PublicationRequestDto request);
        Task<PublicationEditDto?> GetPublicationForEditAsync(int publicationId);
        Task<ServiceResult> UpdatePublicationAsync(PublicationEditDto publication);
        Task<List<string>> GetAllKeywordsAsync();
        Task<List<string>> GetKeywordsForPublicationAsync(int publicationId);
    }
}