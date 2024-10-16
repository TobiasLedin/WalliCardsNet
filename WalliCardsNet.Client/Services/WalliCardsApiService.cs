using System.Net.Http.Json;
using WalliCardsNet.Client.Models;
using static System.Net.WebRequestMethods;

namespace WalliCardsNet.Client.Services
{
    public class WalliCardsApiService
    {
        private readonly HttpClient _httpClient;

        public WalliCardsApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("WalliCardsApi");
        }

        public async Task<ApiResponse<T>> GetByIdAsync<T>(string endpoint, Guid id)     //TODO: Id som string eller Guid??
        {
            var response = await _httpClient.GetAsync($"/api/{endpoint}/{id}");
            return await ProcessResponse<T>(response);
        }

        public async Task<ApiResponse<T>> GetByTokenAsync<T>(string endpoint, string token)
        {
            var response = await _httpClient.GetAsync($"/api/{endpoint}/{token}");
            return await ProcessResponse<T>(response);
        }

        public async Task<ApiResponse<T>> GetAllAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync($"/api/{endpoint}");
            return await ProcessResponse<T>(response);
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, T data)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/{endpoint}", data);
            return await ProcessResponse<T>(response);
        }

        public async Task<HttpResponseMessage> PostFormUrlEncodedAsync(string endpoint, Dictionary<string, string> formData)
        {
            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(endpoint, content);
            return response;
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, T data)
        {
            var apiResponse = new ApiResponse<T>();
            var response = await _httpClient.PutAsJsonAsync<T>($"api/{endpoint}", data);
            if (response.IsSuccessStatusCode)
            {
                apiResponse.IsSuccess = true;
                apiResponse.Message = "Successfully updated";
            }
            else
            {
                apiResponse.IsSuccess = false;
                apiResponse.Message = $"{response.StatusCode} - Error updating";
            }
            return apiResponse;
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint, Guid id)
        {
            var apiResponse = new ApiResponse<T>();
            var response = await _httpClient.DeleteAsync($"api/{endpoint}/{id}");
            if (response.IsSuccessStatusCode)
            {
                apiResponse.IsSuccess = true;
                apiResponse.Message = $"Successfully deleted";
            }
            else
            {
                apiResponse.IsSuccess = false;
                apiResponse.Message = $"{response.StatusCode} - Error deleting";
            }
            return apiResponse;
        }

        public async Task<string> LinkGoogleAccountAsync(string code)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/link/google", code);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            throw new Exception("Error authenticating with Google.");
        }

        private async Task<ApiResponse<T>> ProcessResponse<T>(HttpResponseMessage response)
        {
            var apiResponse = new ApiResponse<T>();
            if (response.IsSuccessStatusCode)
            {
                apiResponse.IsSuccess = true;
                apiResponse.Data = await response.Content.ReadFromJsonAsync<T>();
            }
            else
            {
                apiResponse.IsSuccess = false;
                apiResponse.Message = $"{response.StatusCode} - Error occured";
            }
            return apiResponse;
        }
    }
}
