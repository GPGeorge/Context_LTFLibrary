using LTF_Library_V1.DTOs;


using System.ComponentModel.DataAnnotations;

namespace LTF_Library_V1.DTOs
{
    public class PublicationSearchCriteria
    {
        public string? Title
        {
            get; set;
        }
        public int? CreatorId
        {
            get; set;
        }
        public int? GenreId
        {
            get; set;
        }
        public int? MediaTypeId
        {
            get; set;
        }
        public string? Keyword
        {
            get; set;
        }
        public string? Publisher
        {
            get; set;
        }
        public string? YearPublished
        {
            get; set;
        }
        public string? ISBN
        {
            get; set;
        }
    }
}