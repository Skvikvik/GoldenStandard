using GoldenStandard.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GoldenStandard.Services;

public class ReviewService
{
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task<(bool result, string error)> AddReviewAsync(Review review)
    {
        try
        {
            var resp = await _httpClient.PostAsJsonAsync($"{ApiService.BaseUrl}/api/reviews/", review);
            return (resp.IsSuccessStatusCode, resp.ReasonPhrase ?? "");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
    public async Task<List<Review>> GetReviewsAsync(int productId)
    {
        using var client = new HttpClient();

        var url = $"{ApiService.BaseUrl}/api/goods/reviews/{productId}";

        try
        {
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Review>>() ?? new List<Review>();
            }

            var error = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Ошибка отзывов: {response.StatusCode} - {error}");
            return new List<Review>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка в ReviewService: {ex.Message}");
            return new List<Review>();
        }
    }
}