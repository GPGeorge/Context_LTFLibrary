// Services/RequestManagementService.cs
using Microsoft.EntityFrameworkCore;
using LTF_Library_V1.Data;
using LTF_Library_V1.DTOs;
using LTF_Library_V1.Services;

namespace LTF_Library_V1.Services
{
    public class RequestManagementService : IRequestManagementService
    {
        // CHANGED: Use IDbContextFactory instead of direct DbContext injection
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<RequestManagementService> _logger;

        public RequestManagementService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<RequestManagementService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<PendingRequestDto>> GetPendingRequestsAsync()
        {
            try
            {
                // CHANGED: Create a new context instance for this operation
                await using var context = await _contextFactory.CreateDbContextAsync();

                var pendingRequests = await context.PendingPublicRequests
                    .Select(pr => new PendingRequestDto
                    {
                        RequestID = pr.RequestID,
                        PublicationTitle = pr.PublicationTitle,
                        FirstName = pr.FirstName,
                        LastName = pr.LastName,
                        Email = pr.Email,
                        Phone = pr.Phone,
                        ResearchPurpose = pr.ResearchPurpose,
                        AdditionalInfo = pr.AdditionalInfo,
                        Status = pr.Status,
                        RequestDate = pr.RequestDate
                    })
                    .ToListAsync();

                return pendingRequests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending requests");
                return new List<PendingRequestDto>();
            }
        }

        public async Task<RequestProcessingResult> ProcessRequestAsync(ProcessRequestDto processRequest)
        {
            try
            {
                // CHANGED: Create a new context instance for this operation
                await using var context = await _contextFactory.CreateDbContextAsync();

                var request = await context.PublicationRequests
                    .FirstOrDefaultAsync(r => r.RequestID == processRequest.RequestID);

                if (request == null)
                {
                    return new RequestProcessingResult
                    {
                        Success = false,
                        Message = "Request not found"
                    };
                }

                // Map action to status
                var newStatus = processRequest.Action switch
                {
                    "Approve" => "Approved",
                    "Deny" => "Denied",
                    "RequestInfo" => "Additional Information Requested",
                    _ => request.Status
                };

                request.Status = newStatus;
                request.ProcessedBy = processRequest.ProcessedBy;
                request.ProcessedDate = DateTime.Now;
                request.AdminNotes = processRequest.AdminNotes;

                await context.SaveChangesAsync();

                // Send email notification (implement as needed)
                await SendRequestStatusEmailAsync(request.RequestID, newStatus, processRequest.AdminNotes);

                return new RequestProcessingResult
                {
                    Success = true,
                    Message = $"Request {processRequest.Action.ToLower()}d successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request {RequestId}", processRequest.RequestID);
                return new RequestProcessingResult
                {
                    Success = false,
                    Message = "An error occurred while processing the request"
                };
            }
        }

        public async Task<PendingRequestDto?> GetRequestByIdAsync(int requestId)
        {
            try
            {
                // CHANGED: Create a new context instance for this operation
                await using var context = await _contextFactory.CreateDbContextAsync();

                var request = await context.PublicationRequests
                    .Include(r => r.Publication)
                    .FirstOrDefaultAsync(r => r.RequestID == requestId);

                if (request == null)
                    return null;

                return new PendingRequestDto
                {
                    RequestID = request.RequestID,
                    PublicationTitle = request.Publication?.PublicationTitle ?? "Unknown",
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    ResearchPurpose = request.ResearchPurpose,
                    AdditionalInfo = request.AdditionalInfo,
                    Status = request.Status,
                    RequestDate = request.RequestDate,
                    RequestType = request.RequestType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting request by ID {RequestId}", requestId);
                return null;
            }
        }

        public async Task<List<PendingRequestDto>> GetRequestsByStatusAsync(string status)
        {
            try
            {
                // CHANGED: Create a new context instance for this operation
                await using var context = await _contextFactory.CreateDbContextAsync();

                var requests = await context.PublicationRequests
                    .Include(r => r.Publication)
                    .Where(r => r.Status == status)
                    .Select(r => new PendingRequestDto
                    {
                        RequestID = r.RequestID,
                        PublicationTitle = r.Publication!.PublicationTitle,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Email = r.Email,
                        Phone = r.Phone,
                        ResearchPurpose = r.ResearchPurpose,
                        AdditionalInfo = r.AdditionalInfo,
                        Status = r.Status,
                        RequestDate = r.RequestDate,
                        RequestType = r.RequestType
                    })
                    .OrderByDescending(r => r.RequestDate)
                    .ToListAsync();

                return requests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting requests by status {Status}", status);
                return new List<PendingRequestDto>();
            }
        }

        public async Task<bool> SendRequestStatusEmailAsync(int requestId, string status, string? adminNotes = null)
        {
            try
            {
                // TODO: Implement email sending logic
                // This would integrate with your email service (SendGrid, SMTP, etc.)

                _logger.LogInformation("Email notification sent for request {RequestId} with status {Status}",
                    requestId, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email for request {RequestId}", requestId);
                return false;
            }
        }
    }
}