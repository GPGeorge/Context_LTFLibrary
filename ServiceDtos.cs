// File: ServiceDtos.cs
// General-purpose DTOs used across multiple services

namespace LTF_Library_V1.DTOs
{
    /// <summary>
    /// Standard result wrapper for service operations
    /// </summary>
    public class ServiceResult
    {
        public bool Success
        {
            get; set;
        }
        public string Message { get; set; } = string.Empty;
        public object? Data
        {
            get; set;
        }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        /// <param name="message">Success message</param>
        /// <param name="data">Optional data to return</param>
        /// <returns>ServiceResult with Success = true</returns>
        public static ServiceResult Successful(string message = "Operation completed successfully", object? data = null)
        {
            return new ServiceResult { Success = true, Message = message, Data = data };
        }

        /// <summary>
        /// Creates a failed result
        /// </summary>
        /// <param name="message">Error message</param>
        /// <returns>ServiceResult with Success = false</returns>
        public static ServiceResult Failed(string message)
        {
            return new ServiceResult { Success = false, Message = message };
        }
    }

    /// <summary>
    /// Generic result wrapper for service operations that return typed data
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ServiceResult<T>
    {
        public bool Success
        {
            get; set;
        }
        public string Message { get; set; } = string.Empty;
        public T? Data
        {
            get; set;
        }

        /// <summary>
        /// Creates a successful result with typed data
        /// </summary>
        /// <param name="data">Data to return</param>
        /// <param name="message">Success message</param>
        /// <returns>ServiceResult<T> with Success = true</returns>
        public static ServiceResult<T> Successful(T data, string message = "Operation completed successfully")
        {
            return new ServiceResult<T> { Success = true, Message = message, Data = data };
        }

        /// <summary>
        /// Creates a failed result
        /// </summary>
        /// <param name="message">Error message</param>
        /// <returns>ServiceResult<T> with Success = false</returns>
        public static ServiceResult<T> Failed(string message)
        {
            return new ServiceResult<T> { Success = false, Message = message };
        }
    }

    /// <summary>
    /// Base class for pagination requests
    /// </summary>
    public class PaginationRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = string.Empty;
        public bool SortDescending { get; set; } = false;
    }

    /// <summary>
    /// Base class for paginated responses
    /// </summary>
    /// <typeparam name="T">Type of items in the result</typeparam>
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount
        {
            get; set;
        }
        public int Page
        {
            get; set;
        }
        public int PageSize
        {
            get; set;
        }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}