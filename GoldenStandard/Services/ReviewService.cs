using GoldenStandard.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GoldenStandard.Services;

public class ReviewService
{
    public async Task<(bool result, string error)> AddReviewAsync(int productId, string text, int rating)
    {
        try
        {
            using var client = new HttpClient();
            ApiService.Authenticate(client); // Токен обязателен

            var url = $"{ApiService.BaseUrl}/api/goods/reviews/{productId}";
            var payload = new { text = text, rating = rating };

            var resp = await client.PostAsJsonAsync(url, payload);

            if (resp.IsSuccessStatusCode)
                return (true, "");

            var errorContent = await resp.Content.ReadAsStringAsync();
            return (false, $"Server error: {resp.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<List<Review>> GetReviewsAsync(int productId)
    {
        try
        {
            using var client = new HttpClient();
            // Если получение отзывов тоже требует авторизации, раскомментируй строку ниже:
            // ApiService.Authenticate(client); 

            var url = $"{ApiService.BaseUrl}/api/goods/reviews/{productId}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Review>>() ?? new List<Review>();
            }
            return new List<Review>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ReviewService Get error: {ex.Message}");
            return new List<Review>();
        }
    }
}