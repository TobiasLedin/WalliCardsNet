using System.Net.Http.Json;
using WalliCardsNet.Client.Models;

namespace WalliCardsNet.Client.Services
{
    public class WalliCardsApiService
    {
        private readonly HttpClient _httpClient;

        public WalliCardsApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("WalliCardsApi");
        }

        public async Task<ApiResponse<T>> GetByIdAsync<T>(string endpoint, string id)
        {
            var response = await _httpClient.GetAsync($"/api/{endpoint}/{id}");
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
