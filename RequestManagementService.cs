// Services/RequestManagementService.cs - Alternative approach without DbContextFactory
using LTF_Library_V1.Data;
using LTF_Library_V1.DTOs;
using LTF_Library_V1.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LTF_Library_V1.Services
{
    public class RequestManagementService : IRequestManagementService
    {
        // CHANGED: Use IServiceProvider to create scoped contexts
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RequestManagementService> _logger;
        private readonly string _connectionString;

        public RequestManagementService(
            IServiceProvider serviceProvider,
            ILogger<RequestManagementService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<List<PendingRequestDto>> GetPendingRequestsAsync()
        {
            try
            {
                // Create a new scope for each operation to avoid concurrency
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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
                        RequestDate = pr.RequestDate,
                        RequestType = pr.RequestType
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
                // Map action to status
                var newStatus = processRequest.Action switch
                {
                    "Approve" => "Approved",
                    "Deny" => "Denied",
                    "RequestInfo" => "Additional Information Requested",
                    _ => "Pending"
                };

                // Call the stored procedure to update the status
                var success = await UpdateRequestStatusAsync(
                    processRequest.RequestID,
                    newStatus,
                    processRequest.ProcessedBy,
                    processRequest.AdminNotes
                );

                if (!success)
                {
                    return new RequestProcessingResult
                    {
                        Success = false,
                        Message = "Failed to update request status"
                    };
                }

                // Send email notification
                await SendRequestStatusEmailAsync(processRequest.RequestID, newStatus, processRequest.AdminNotes);

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

        public async Task<bool> UpdateRequestStatusAsync(int requestId, string status, string processedBy, string? adminNotes)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand("sp_UpdateRequestStatus", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@RequestID", requestId);
                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.AddWithValue("@ProcessedBy", processedBy);
                command.Parameters.AddWithValue("@ProcessedDate", DateTime.Now);
                command.Parameters.AddWithValue("@AdminNotes", adminNotes ?? (object)DBNull.Value);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request status for RequestID {RequestId}", requestId);
                return false;
            }
        }
        public async Task<PendingRequestDto?> GetRequestByIdAsync(int requestId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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
                // When implemented, this will likely use something like:
                // await _emailService.SendAsync(...);
                _logger.LogInformation("Email notification sent for request {RequestId} with status {Status}",
                    requestId, status);
                await Task.CompletedTask;
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