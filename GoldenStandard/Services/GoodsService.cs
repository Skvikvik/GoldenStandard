using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using GoldenStandard.Models;

namespace GoldenStandard.Services;

public class GoodsService
{
    private readonly HttpClient _client;

    public GoodsService()
    {
        _client = new HttpClient();
        ApiService.Authenticate(_client);
    }

    public async Task<List<Product>> GetProductsAsync(int offset = 0, int limit = 20)
    {
        try
        {
            var url = $"{ApiService.BaseUrl}/api/goods/?offset={offset}&limit={limit}";
            var products = await _client.GetFromJsonAsync<List<Product>>(url);
            if (products != null)
            {
                foreach (var p in products) FixAndNotify(p);
                return products;
            }
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}"); }
        return new List<Product>();
    }

    public async Task<Product?> GetProductDetailsAsync(int id)
    {
        try
        {
            var product = await _client.GetFromJsonAsync<Product>($"{ApiService.BaseUrl}/api/goods/{id}/");
            if (product != null) FixAndNotify(product);
            return product;
        }
        catch { return null; }
    }

    public async Task<bool> AddProductAsync(string name, string composition, decimal price, string imageUrl)
    {
        var payload = new { name, composition, description = composition, price, image_url = imageUrl };
        var response = await _client.PostAsJsonAsync($"{ApiService.BaseUrl}/api/goods/", payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProductAsync(int id, string name, string composition, decimal price, string imageUrl)
    {
        var payload = new { name, composition, description = composition, price, image_url = imageUrl };
        var response = await _client.PutAsJsonAsync($"{ApiService.BaseUrl}/api/goods/{id}/", payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var response = await _client.DeleteAsync($"{ApiService.BaseUrl}/api/goods/{id}/");
        return response.IsSuccessStatusCode;
    }

    private void FixAndNotify(Product product)
    {
        if (product == null) return;
        var fixedReviews = product.Reviews?.Select(r => {
            if (r.User == null || string.IsNullOrWhiteSpace(r.User.Username) || r.User.Username == "guest")
                r.User = new User { Username = "Гость" };
            return r;
        }).ToList() ?? new List<Review>();

        product.Reviews = new ObservableCollection<Review>(fixedReviews);
        product.RefreshRatingUI();
    }
}