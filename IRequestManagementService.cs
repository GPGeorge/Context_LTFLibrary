
// Services/IRequestManagementService.cs
using LTF_Library_V1.DTOs;

namespace LTF_Library_V1.Services
{
    public interface IRequestManagementService
    {
        Task<List<PendingRequestDto>> GetPendingRequestsAsync();
        Task<RequestProcessingResult> ProcessRequestAsync(ProcessRequestDto processRequest);
        Task<PendingRequestDto?> GetRequestByIdAsync(int requestId);
        Task<List<PendingRequestDto>> GetRequestsByStatusAsync(string status);
        Task<bool> SendRequestStatusEmailAsync(int requestId, string status, string? adminNotes = null);
        Task<bool> UpdateRequestStatusAsync(int requestId, string status, string processedBy, string? adminNotes);
    }
}