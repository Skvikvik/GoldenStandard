using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GoldenStandard.Models;

namespace GoldenStandard.Services;

public static class ApiService
{
    public const string BaseUrl = "http://sarafan.bgig.ru"; // Твой основной хост
    public static string? AccessToken { get; set; }

    public static void Authenticate(HttpClient client)
    {
        if (!string.IsNullOrEmpty(AccessToken))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AccessToken);
        }
    }

    public static async Task<User?> GetProfileAsync()
    {
        try
        {
            using var client = new HttpClient();
            Authenticate(client);
            // Запрос именно профиля текущего юзера
            var response = await client.GetAsync($"{BaseUrl}/api/user/profile");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<User>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiService] Profile Error: {ex.Message}");
        }
        return null;
    }
}