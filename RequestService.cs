// Services/RequestService.cs - Your existing service
using LTF_Library_V1.DTOs;
using Microsoft.Data.SqlClient;
using System.Data;
using static LTF_Library_V1.Pages.PublicationDetail;

namespace LTF_Library_V1.Services
{
    public class RequestService : IRequestService
    {
        private readonly string _connectionString;

        public RequestService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> SubmitRequestAsync(RequestFormModel request, PublicationDetailDto publication)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand("sp_SubmitPublicationRequest", connection);

                command.CommandType = CommandType.StoredProcedure;

                // Add parameters for the stored procedure
                command.Parameters.AddWithValue("@PublicationId", publication.PublicationID);
                command.Parameters.AddWithValue("@FirstName", request.FirstName);
                command.Parameters.AddWithValue("@LastName", request.LastName);
                command.Parameters.AddWithValue("@Email", request.Email);
                command.Parameters.AddWithValue("@Phone", request.Phone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ResearchPurpose", request.ResearchPurpose);
                command.Parameters.AddWithValue("@RequestType", request.RequestType);
                command.Parameters.AddWithValue("@AdditionalInfo", request.AdditionalInfo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@RequestDate", DateTime.Now);
                command.Parameters.AddWithValue("@Status", "Pending");

                // Add output parameter to get the new request ID
                var requestIdParam = new SqlParameter("@RequestId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(requestIdParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                // You could use the returned RequestId if needed
                var requestId = (int)requestIdParam.Value;

                return true;
            }
            catch (Exception ex)
            {
                // Log the error (you might want to use ILogger here)
                //console.writeLine($"Error submitting request: {ex.Message}");
                return false;
            }
        }

        public async Task<List<RequestDto>> GetRequestsAsync()
        {
            var requests = new List<RequestDto>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand("sp_GetAllRequests", connection);

                command.CommandType = CommandType.StoredProcedure;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    requests.Add(new RequestDto
                    {
                        Id = reader.GetInt32("RequestId"),
                        PublicationId = reader.GetInt32("PublicationId"),
                        PublicationTitle = reader.GetString("PublicationTitle"),
                        FirstName = reader.GetString("FirstName"),
                        LastName = reader.GetString("LastName"),
                        Email = reader.GetString("Email"),
                        Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                        ResearchPurpose = reader.GetString("ResearchPurpose"),
                        RequestType = reader.GetString("RequestType"),
                        AdditionalInfo = reader.IsDBNull("AdditionalInfo") ? null : reader.GetString("AdditionalInfo"),
                        RequestDate = reader.GetDateTime("RequestDate"),
                        Status = reader.GetString("Status")
                    });
                }
            }
            catch (Exception ex)
            {
                //console.writeLine($"Error getting requests: {ex.Message}");
            }

            return requests;
        }

        public async Task<RequestDto> GetRequestByIdAsync(int requestId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand("sp_GetRequestById", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@RequestId", requestId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new RequestDto
                    {
                        Id = reader.GetInt32("RequestId"),
                        PublicationId = reader.GetInt32("PublicationId"),
                        PublicationTitle = reader.GetString("PublicationTitle"),
                        FirstName = reader.GetString("FirstName"),
                        LastName = reader.GetString("LastName"),
                        Email = reader.GetString("Email"),
                        Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                        ResearchPurpose = reader.GetString("ResearchPurpose"),
                        RequestType = reader.GetString("RequestType"),
                        AdditionalInfo = reader.IsDBNull("AdditionalInfo") ? null : reader.GetString("AdditionalInfo"),
                        RequestDate = reader.GetDateTime("RequestDate"),
                        Status = reader.GetString("Status")
                    };
                }
            }
            catch (Exception ex)
            {
                //console.writeLine($"Error getting request by ID: {ex.Message}");
            }

            return null;
        }
    }
}