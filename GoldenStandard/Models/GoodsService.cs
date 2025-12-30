using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GoldenStandard.Models;

namespace GoldenStandard.Services;

public class GoodsService
{
    public async Task<List<Product>> GetProductsAsync(int offset = 0, int limit = 20)
    {
        using var client = new HttpClient();
        ApiService.Authenticate(client);
        var url = $"{ApiService.BaseUrl}/api/goods/?offset={offset}&limit={limit}";
        try
        {
            return await client.GetFromJsonAsync<List<Product>>(url) ?? new();
        }
        catch { return new(); }
    }

    public async Task<Product?> GetProductDetailsAsync(int id)
    {
        using var client = new HttpClient();
        ApiService.Authenticate(client);
        try
        {
            return await client.GetFromJsonAsync<Product>($"{ApiService.BaseUrl}/api/goods/{id}");
        }
        catch { return null; }
    }

    public async Task<bool> AddProductAsync(string name, string description, decimal price, string imageUrl)
    {
        using var client = new HttpClient();
        ApiService.Authenticate(client);

        var url = $"{ApiService.BaseUrl}/api/goods/";

        var payload = new
        {
            name = name,
            description = description,
            price = price,
            image_url = string.IsNullOrEmpty(imageUrl) ? null : imageUrl,
            composition = description
        };

        try
        {
            var response = await client.PostAsJsonAsync(url, payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка API: {ex.Message}");
            return false;
        }
    }
}