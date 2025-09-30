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