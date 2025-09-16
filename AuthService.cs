using LTF_Library_V1.Data.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace LTF_Library_V1.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        public WhoAmIResult? CurrentUser
        {
            get; private set;
        }

        public async Task<bool> CheckAuthenticationAsync()
        {
            try
            {
                var result = await _http.GetFromJsonAsync<WhoAmIResult>("/Account/whoami");
                CurrentUser = result;
                return result?.Success ?? false;
            }
            catch
            {
                CurrentUser = null;
                return false;
            }
        }

        public string? Username => CurrentUser?.Username;
    }
}
