using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GoldenStandard.Models;
using System.Linq;

namespace GoldenStandard.Services;

public class GoodsService
{
    public async Task<List<Product>> GetProductsAsync(int offset = 0, int limit = 20)
    {
        using var client = new HttpClient();
        ApiService.Authenticate(client);

        // Запрашиваем чистый список, так как сервер не поддерживает фильтрацию через URL
        var url = $"{ApiService.BaseUrl}/api/goods/?offset={offset}&limit={limit}";

        try
        {
            System.Diagnostics.Debug.WriteLine($"[API Request]: {url}");
            var products = await client.GetFromJsonAsync<List<Product>>(url);

            if (products != null)
            {
                foreach (var product in products) FixAndNotify(product);
                return products;
            }
            return new List<Product>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error]: {ex.Message}");
            return new List<Product>();
        }
    }

    public async Task<Product?> GetProductDetailsAsync(int id)
    {
        using var client = new HttpClient();
        ApiService.Authenticate(client);
        try
        {
            var product = await client.GetFromJsonAsync<Product>($"{ApiService.BaseUrl}/api/goods/{id}/");
            if (product != null) FixAndNotify(product);
            return product;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка деталей: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> AddProductAsync(string name, string composition, decimal price, string imageUrl)
    {
        using var client = new HttpClient();
        ApiService.Authenticate(client);
        var payload = new { name, composition, description = composition, price, image_url = imageUrl };
        try
        {
            var response = await client.PostAsJsonAsync($"{ApiService.BaseUrl}/api/goods/", payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка отправки: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        using var client = new HttpClient();
        ApiService.Authenticate(client);
        try
        {
            var response = await client.DeleteAsync($"{ApiService.BaseUrl}/api/goods/{id}/");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка удаления: {ex.Message}");
            return false;
        }
    }

    private void FixAndNotify(Product product)
    {
        if (product == null) return;
        var fixedReviews = product.Reviews?.Select(r => {
            if (r.User == null) r.User = new User { Username = "Гость" };
            return r;
        }).ToList() ?? new List<Review>();
        product.Reviews = new ObservableCollection<Review>(fixedReviews);
        product.RefreshRatingUI();
    }
}