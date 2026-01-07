using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GoldenStandard.Models;

namespace GoldenStandard.Services;

public class GoodsService
{
    // ИСПРАВЛЕНО: Добавлен параметр search. 
    // Если поиск не передан, он будет пустой строкой по умолчанию.
    public async Task<List<Product>> GetProductsAsync(int offset = 0, string search = "", int limit = 20)
    {
        using var client = new HttpClient();
        ApiService.Authenticate(client);

        // Формируем URL с учетом поиска. Uri.EscapeDataString нужен для корректной передачи пробелов/кириллицы.
        var url = $"{ApiService.BaseUrl}/api/goods/?offset={offset}&limit={limit}";

        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }

        try
        {
            // Возвращаем пустой список вместо null в случае неудачи (?? new())
            return await client.GetFromJsonAsync<List<Product>>(url) ?? new List<Product>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при получении списка: {ex.Message}");
            return new List<Product>();
        }
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