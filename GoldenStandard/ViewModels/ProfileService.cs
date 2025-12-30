using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GoldenStandard.Models;

namespace GoldenStandard.Services;

public class ProfileService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = ApiService.BaseUrl;

    public ProfileService()
    {
        _httpClient = new HttpClient();
    }

    private void AddAuthHeader()
    {
        if (!string.IsNullOrEmpty(ApiService.AccessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ApiService.AccessToken);
        }
    }

    public async Task<UserProfile?> GetMyProfileAsync()
    {
        if (ApiService.AccessToken == "offline_access_token")
        {
            return new UserProfile
            {
                user_id = 0,
                username = "admin",
                display_name = "Администратор (Оффлайн)",
                description = "Локальная учетная запись"
            };
        }

        AddAuthHeader();

        AddAuthHeader();
        try
        {
            return await _httpClient.GetFromJsonAsync<UserProfile>($"{_baseUrl}/api/profiles/profile/");
        }
        catch { return null; }

    }

    public async Task<UserProfile?> UpdateProfileAsync(UserProfile updatedData)
    {
        AddAuthHeader();
        var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/profiles/profile/", updatedData);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<UserProfile>();

        return null;
    }

    public async Task<bool> DeleteProfileAsync()
    {
        AddAuthHeader();
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/profiles/profile/");
        return response.IsSuccessStatusCode;
    }

    public async Task<UserProfile?> GetUserProfileAsync(int userId)
    {
        AddAuthHeader();
        try
        {
            return await _httpClient.GetFromJsonAsync<UserProfile>($"{_baseUrl}/api/profiles/profile/{userId}");
        }
        catch { return null; }
    }
}